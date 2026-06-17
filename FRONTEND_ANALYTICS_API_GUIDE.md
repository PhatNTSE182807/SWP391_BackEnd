# 📊 Analytics API — Frontend Integration Guide

Tài liệu này cung cấp chi tiết đặc tả API (API Spec), cấu trúc JSON trả về, và cách cấu hình các thư viện vẽ biểu đồ ở Frontend để dựng 13 loại thống kê phân tích.

---

## 🛠️ Thư viện vẽ biểu đồ khuyến nghị (React/Next.js)

Để vẽ các loại biểu đồ đẹp và hiệu năng cao từ dữ liệu backend, khuyến nghị sử dụng các thư viện sau:
1. **Biểu đồ thông thường (Line, Bar, Donut)**: 
   * **[Recharts](https://recharts.org/)** (Khuyến nghị cho React/Next.js - Dễ cấu hình, giao diện hiện đại).
   * **[Chart.js](https://www.chartjs.org/)** (hoặc wrapper `react-chartjs-2`).
2. **Biểu đồ đám mây từ khóa (Word Cloud)**:
   * **[react-wordcloud](https://github.com/chrisrzhou/react-wordcloud)** (Dựa trên D3, tự động scale kích thước chữ rất mượt).
3. **Mạng lưới liên kết (Network Graph)**:
   * **[react-force-graph](https://github.com/vasturiano/react-force-graph)** (Đẹp mắt, hỗ trợ kéo thả và zoom 2D/3D cực mạnh mẽ).
   * **[vis-network](https://visjs.github.io/vis-network/)** (Rất phổ biến, nhẹ và tương thích tốt).

---

## 🔑 Cấu trúc dữ liệu chung (Common Formats)

Backend trả về dữ liệu qua lớp bọc `ApiResult` chuẩn của hệ thống:
```json
{
  "success": true,
  "data": ..., // Dữ liệu thống kê nằm ở đây
  "errors": null
}
```

Dữ liệu thống kê ở trường `data` sẽ thuộc 1 trong 3 dạng cấu trúc chuẩn hóa sau:

### Dạng 1: Simple Series Data (Dùng cho Line, Bar, Donut, Word Cloud)
```json
[
  { "key": "2024", "value": 150.0 },
  { "key": "2025", "value": 210.0 }
]
```

### Dạng 2: Multi-Series Data (Dùng cho Multi-line, Stacked Bar)
```json
[
  {
    "seriesName": "Machine Learning",
    "dataPoints": [
      { "key": "2023", "value": 45.0 },
      { "key": "2024", "value": 80.0 }
    ]
  }
]
```

### Dạng 3: Network Graph Data (Dùng cho Mạng lưới liên kết)
```json
{
  "nodes": [
    { "id": "A", "label": "Nguyễn Văn A", "size": 42.0, "group": "Author" },
    { "id": "B", "label": "Trần Thị B", "size": 15.0, "group": "Author" }
  ],
  "edges": [
    { "source": "A", "target": "B", "weight": 5.0 }
  ]
}
```

---

## 📈 Chi tiết 13 API thống kê và cấu hình Frontend

### 1. Số bài báo theo năm (Line chart)
* **API**: `GET /api/analytics/trends/papers-by-year`
* **Phân quyền**: Yêu cầu Token (`[Authorize]`)
* **Dữ liệu trả về**: `Simple Series Data`
* **Tích hợp Recharts**:
  ```jsx
  <ResponsiveContainer width="100%" height={300}>
    <LineChart data={apiData}>
      <XAxis dataKey="key" />
      <YAxis />
      <Tooltip />
      <Line type="monotone" dataKey="value" stroke="#8884d8" strokeWidth={2} />
    </LineChart>
  </ResponsiveContainer>
  ```

### 2. Citation tăng trưởng theo năm (Line chart)
* **API**: `GET /api/analytics/trends/citations-by-year`
* **Dữ liệu trả về**: `Simple Series Data`
* **Tích hợp**: Vẽ tương tự biểu đồ số 1, biểu thị tổng số lượt trích dẫn thu thập được qua các năm.

### 3. Top topic nổi bật (Bar chart)
* **API**: `GET /api/analytics/trends/top-topics?size=10`
* **Params**: `size` (mặc định: 10)
* **Dữ liệu trả về**: `Simple Series Data`
* **Tích hợp Recharts**:
  ```jsx
  <BarChart data={apiData} layout="vertical">
    <XAxis type="number" />
    <YAxis dataKey="key" type="category" width={150} />
    <Tooltip />
    <Bar dataKey="value" fill="#82ca9d" radius={[0, 4, 4, 0]} />
  </BarChart>
  ```

### 4. Top domain nổi bật (Bar chart)
* **API**: `GET /api/analytics/trends/top-domains?size=10`
* **Dữ liệu trả về**: `Simple Series Data` (Biểu thị số lượng bài nghiên cứu nằm trong mỗi phân vùng lĩnh vực/domain).

### 5. Keyword trending theo thời gian (Multi-line chart)
* **API**: `GET /api/analytics/trends/keywords-over-time`
* **Params**: `keywords` (Mảng từ khóa cần lọc, ví dụ: `?keywords=ai&keywords=iot`). Nếu bỏ trống, Backend tự động trả về 5 từ khóa hot nhất.
* **Dữ liệu trả về**: `Multi-Series Data`
* **Cách map data để Recharts hiểu**:
  Frontend cần chuyển đổi cấu trúc `Multi-Series` thành dạng phẳng (flattened by year):
  ```javascript
  const years = [...new Set(apiData.flatMap(s => s.dataPoints.map(dp => dp.key)))].sort();
  const chartData = years.map(year => {
    const row = { year };
    apiData.forEach(series => {
      const point = series.dataPoints.find(dp => dp.key === year);
      row[series.seriesName] = point ? point.value : 0;
    });
    return row;
  });
  ```
  **Vẽ biểu đồ**:
  ```jsx
  <LineChart data={chartData}>
    <XAxis dataKey="year" />
    <YAxis />
    <Tooltip />
    {apiData.map(series => (
      <Line key={series.seriesName} type="monotone" dataKey={series.seriesName} stroke={getRandomColor()} />
    ))}
  </LineChart>
  ```

### 6. Top tác giả nhiều citation nhất (Bar chart)
* **API**: `GET /api/analytics/authors/top-citations?size=10`
* **Dữ liệu trả về**: `Simple Series Data` (Tên tác giả làm `key`, tổng lượt trích dẫn làm `value`).

### 7. Top tác giả H-index cao nhất (Bar chart)
* **API**: `GET /api/analytics/authors/top-hindex?size=10`
* **Dữ liệu trả về**: `Simple Series Data` (Tên tác giả làm `key`, H-Index làm `value`).

### 8. Mạng lưới cộng tác tác giả (Network Graph)
* **API**: `GET /api/analytics/authors/collaboration-network?size=50`
* **Params**: `size` (Lọc số lượng tác giả hiển thị để tránh rối mắt, mặc định: 50).
* **Dữ liệu trả về**: `Network Graph Data`
* **Tích hợp react-force-graph**:
  ```jsx
  import { ForceGraph2D } from 'react-force-graph';
  
  <ForceGraph2D
    graphData={apiData}
    nodeId="id"
    nodeVal="size"
    nodeLabel="label"
    nodeColor={() => '#ff7300'}
    linkSource="source"
    linkTarget="target"
    linkWidth={link => Math.sqrt(link.weight)}
  />
  ```

### 9. Top journal nhiều bài nhất (Bar chart)
* **API**: `GET /api/analytics/journals/top-paper-count?size=10`
* **Dữ liệu trả về**: `Simple Series Data` (Tên Journal làm `key`, số bài báo làm `value`).

### 10. Top journal có citation cao nhất (Bar chart)
* **API**: `GET /api/analytics/journals/top-citations?size=10`
* **Dữ liệu trả về**: `Simple Series Data`

### 11. Tỉ lệ Open Access vs Closed (Donut/Pie chart)
* **API**: `GET /api/analytics/journals/open-access-ratio`
* **Dữ liệu trả về**: `Simple Series Data`
* **Tích hợp Recharts (Pie/Donut)**:
  ```jsx
  const COLORS = ['#0088FE', '#00C49F'];
  
  <PieChart>
    <Pie
      data={apiData}
      dataKey="value"
      nameKey="key"
      innerRadius={60}
      outerRadius={80}
      fill="#8884d8"
      label
    >
      {apiData.map((entry, index) => (
        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
      ))}
    </Pie>
    <Tooltip />
  </PieChart>
  ```

### 12. Word cloud - Keyword xuất hiện nhiều nhất (Word Cloud)
* **API**: `GET /api/analytics/keywords/word-cloud?size=50`
* **Dữ liệu trả về**: `Simple Series Data`
* **Tích hợp react-wordcloud**:
  ```jsx
  import ReactWordcloud from 'react-wordcloud';
  
  const words = apiData.map(item => ({
    text: item.key,
    value: item.value
  }));
  
  <ReactWordcloud words={words} options={{ rotations: 2, rotationAngles: [0, 90] }} />
  ```

### 13. Top keyword theo năm (Stacked/Grouped Bar chart)
* **API**: `GET /api/analytics/keywords/top-by-year?size=10`
* **Params**: `size` (Số lượng keyword hot nhất cần lấy, mặc định: 10)
* **Dữ liệu trả về**: `Multi-Series Data`
* **Tích hợp**: Sử dụng cách map phẳng hóa dữ liệu tương tự biểu đồ số 5 (Keyword trending), nhưng render dưới dạng **Bar Chart** với tùy chọn `stackId="a"` trong Recharts để làm Stacked Bar Chart.

### 14. Keyword Co-occurrence (Network Graph)
* **API**: `GET /api/analytics/keywords/co-occurrence?size=50`
* **Dữ liệu trả về**: `Network Graph Data`
* **Tích hợp**: Sử dụng `react-force-graph` tương tự biểu đồ số 8 (Mạng lưới cộng tác tác giả).
