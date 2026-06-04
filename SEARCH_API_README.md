# Search API Documentation

## Overview
API tìm kiếm full-text cho papers sử dụng Elasticsearch với Redis cache và Hangfire background jobs.

## Endpoints

### 1. Search Papers
**GET** `/api/search/papers`

Full-text search bài báo với highlighting và pagination.

**Authentication:** Required (Bearer Token)

**Query Parameters:**

| Tên | Kiểu | Mô tả | Mặc định |
|-----|------|-------|----------|
| q | string | Từ khóa tìm kiếm | - |
| page | int | Trang | 1 |
| size | int | Số kết quả mỗi trang | 10 |
| from | int | Năm bắt đầu | - |
| to | int | Năm kết thúc | - |
| language | string | Ngôn ngữ | - |
| isOpenAccess | bool | Lọc open access | - |

**Example Request:**
```bash
GET /api/search/papers?q=machine%20learning&page=1&size=10&from=2020&to=2024&isOpenAccess=true
Authorization: Bearer YOUR_JWT_TOKEN
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "total": 100,
    "page": 1,
    "size": 10,
    "results": [
      {
        "paperId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "Machine Learning Applications",
        "abstract": "This paper discusses...",
        "publicationYear": 2024,
        "citedByCount": 42,
        "highlight": {
          "title": ["<em>Machine</em> <em>Learning</em> Applications"],
          "abstract": ["...discusses <em>machine</em> <em>learning</em> techniques..."]
        }
      }
    ]
  },
  "errors": null
}
```

### 2. Reindex Papers (Synchronous)
**POST** `/api/search/reindex`

Trigger bulk indexing đồng bộ tất cả papers vào Elasticsearch. API này sẽ chờ cho đến khi hoàn thành.

**Authentication:** Required (Admin role)

**Response 200:**
```json
{
  "success": true,
  "data": "Reindexing completed successfully",
  "errors": null
}
```

### 3. Reindex Papers (Background Job)
**POST** `/api/search/reindex/background`

Enqueue một Hangfire background job để reindex papers. API trả về ngay lập tức với job ID.

**Authentication:** Required (Admin role)

**Response 200:**
```json
{
  "success": true,
  "data": {
    "jobId": "12345",
    "message": "Reindexing job enqueued successfully"
  },
  "errors": null
}
```

## Features

### 1. Full-Text Search
- Tìm kiếm trên **title** và **abstract**
- Title có trọng số cao hơn (boost ^2)
- Hỗ trợ fuzzy matching (AUTO)
- Operator: OR (tìm bất kỳ từ nào)

### 2. Filters
- **Year Range**: Lọc theo khoảng năm xuất bản
- **Language**: Lọc theo ngôn ngữ
- **Open Access**: Lọc papers open access

### 3. Highlighting
- Highlight từ khóa trong title và abstract
- Sử dụng tag `<em>` để đánh dấu
- Abstract được chia thành fragments (150 chars, max 3 fragments)

### 4. Sorting
- Sắp xếp theo relevance score (mặc định)
- Sau đó sắp xếp theo số lần được trích dẫn (citedByCount)

### 5. Redis Cache
- Cache kết quả tìm kiếm trong **15 phút**
- Cache key dựa trên tất cả query parameters
- Giảm tải cho Elasticsearch

### 6. Hangfire Background Jobs
- **Recurring Job**: Tự động reindex papers mỗi ngày lúc 2 AM
- **Manual Job**: Trigger reindex thủ công qua API
- **Retry**: Tự động retry 3 lần nếu thất bại
- **Monitor**: Xem trạng thái jobs tại `/hangfire`

## Setup

### 1. Elasticsearch Configuration
Cấu hình trong `appsettings.json`:
```json
{
  "Elasticsearch": {
    "Uri": "http://157.66.101.190:9200"
  }
}
```

### 2. Redis Configuration
```json
{
  "Redis": {
    "ConnectionString": "redis:6379,abortConnect=false"
  }
}
```

### 3. Initial Index Creation
Khi chạy lần đầu, cần tạo index và index dữ liệu:

**Option 1: Synchronous (cho dev/test)**
```bash
POST /api/search/reindex
Authorization: Bearer ADMIN_TOKEN
```

**Option 2: Background Job (recommended cho production)**
```bash
POST /api/search/reindex/background
Authorization: Bearer ADMIN_TOKEN
```

Sau đó theo dõi tiến trình tại: `http://your-api/hangfire`

### 4. Automatic Reindexing
Hangfire sẽ tự động reindex papers mỗi ngày lúc 2 AM. Không cần cấu hình thêm.

## Architecture

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │
       ▼
┌─────────────────┐
│ SearchController│
└──────┬──────────┘
       │
       ▼
┌─────────────────┐      ┌──────────────┐
│  SearchService  │─────▶│ Redis Cache  │
└──────┬──────────┘      └──────────────┘
       │
       ▼
┌─────────────────┐
│ Elasticsearch   │
└─────────────────┘

┌──────────────────┐
│ HangfireJobService│
└──────┬───────────┘
       │
       ▼
┌─────────────────┐
│  SearchService  │
│ (BulkIndex)     │
└─────────────────┘
```

## Performance Considerations

1. **Pagination**: Sử dụng `from` và `size` để phân trang, tránh load quá nhiều dữ liệu
2. **Cache**: Redis cache giảm 90% request đến Elasticsearch
3. **Bulk Indexing**: Index theo batch 1000 records để tối ưu performance
4. **Background Jobs**: Reindex chạy background không block API
5. **Index Mapping**: Sử dụng `keyword` cho exact match (language), `text` cho full-text search

## Monitoring

### Hangfire Dashboard
Truy cập: `http://your-api/hangfire`

Xem:
- Recurring jobs schedule
- Job execution history
- Failed jobs và retry attempts
- Server statistics

### Elasticsearch Health
```bash
GET http://157.66.101.190:9200/_cluster/health
```

### Redis Health
```bash
redis-cli ping
```

## Troubleshooting

### 1. No search results
- Kiểm tra Elasticsearch có running không
- Kiểm tra index `papers` đã được tạo chưa
- Chạy reindex để đảm bảo dữ liệu đã được index

### 2. Slow search
- Kiểm tra Redis cache có hoạt động không
- Giảm `size` parameter
- Tối ưu query (bỏ filters không cần thiết)

### 3. Reindex failed
- Kiểm tra connection string đến database
- Kiểm tra Elasticsearch có đủ disk space
- Xem logs trong Hangfire dashboard

## Future Enhancements

1. **Aggregations**: Thêm facets cho filters (count by year, language, etc.)
2. **Suggestions**: Auto-complete và spell checking
3. **More Like This**: Tìm papers tương tự
4. **Advanced Filters**: Filter by author, journal, topics
5. **Export**: Export search results to CSV/JSON
6. **Analytics**: Track popular search queries
