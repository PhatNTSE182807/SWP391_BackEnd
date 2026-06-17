# 📘 Search API - Hướng Dẫn Cho Frontend

## 🔗 Base URL
```
Production: https://your-api.com
Development: http://localhost:5000
```

---

## 🔐 Authentication

Tất cả API đều yêu cầu **JWT Token** trong header:

```javascript
headers: {
  'Authorization': 'Bearer YOUR_JWT_TOKEN',
  'Content-Type': 'application/json'
}
```

**Phân quyền:**
- ✅ **User & Admin**: Có thể search papers
- ❌ **Admin only**: Reindex, recreate index

---

## 🔍 1. Search Papers

### **Endpoint**
```
GET /api/search/papers
```

### **Description**
Full-text search bài báo với Elasticsearch, hỗ trợ highlighting và filters.

### **Authorization**
✅ Required (User hoặc Admin)

### **Query Parameters**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `q` | string | ❌ | - | Từ khóa tìm kiếm (search trong title và abstract) |
| `page` | integer | ❌ | 1 | Số trang (pagination) |
| `size` | integer | ❌ | 10 | Số kết quả mỗi trang (max: 100) |
| `from` | integer | ❌ | - | Năm bắt đầu (VD: 2020) |
| `to` | integer | ❌ | - | Năm kết thúc (VD: 2024) |
| `language` | string | ❌ | - | Ngôn ngữ (VD: "en", "vi") |
| `isOpenAccess` | boolean | ❌ | - | Lọc open access (true/false) |

### **Request Examples**

#### **JavaScript (Fetch)**
```javascript
// Basic search
async function searchPapers(keyword, page = 1) {
  const token = localStorage.getItem('jwt_token');
  
  const response = await fetch(
    `http://localhost:5000/api/search/papers?q=${encodeURIComponent(keyword)}&page=${page}&size=10`,
    {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    }
  );
  
  const data = await response.json();
  return data;
}

// Search with filters
async function searchPapersAdvanced(filters) {
  const token = localStorage.getItem('jwt_token');
  const params = new URLSearchParams();
  
  if (filters.keyword) params.append('q', filters.keyword);
  if (filters.page) params.append('page', filters.page);
  if (filters.size) params.append('size', filters.size);
  if (filters.fromYear) params.append('from', filters.fromYear);
  if (filters.toYear) params.append('to', filters.toYear);
  if (filters.language) params.append('language', filters.language);
  if (filters.isOpenAccess !== undefined) params.append('isOpenAccess', filters.isOpenAccess);
  
  const response = await fetch(
    `http://localhost:5000/api/search/papers?${params.toString()}`,
    {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    }
  );
  
  return await response.json();
}

// Usage
const results = await searchPapersAdvanced({
  keyword: 'machine learning',
  page: 1,
  size: 20,
  fromYear: 2020,
  toYear: 2024,
  language: 'en',
  isOpenAccess: true
});
```

#### **Axios**
```javascript
import axios from 'axios';

const searchPapers = async (keyword, page = 1) => {
  const token = localStorage.getItem('jwt_token');
  
  const response = await axios.get('http://localhost:5000/api/search/papers', {
    params: {
      q: keyword,
      page: page,
      size: 10
    },
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  return response.data;
};
```

#### **React Hook Example**
```javascript
import { useState, useEffect } from 'react';
import axios from 'axios';

function useSearchPapers(keyword, page = 1, filters = {}) {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      setError(null);
      
      try {
        const token = localStorage.getItem('jwt_token');
        const response = await axios.get('/api/search/papers', {
          params: {
            q: keyword,
            page: page,
            size: 10,
            ...filters
          },
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });
        
        setData(response.data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    if (keyword) {
      fetchData();
    }
  }, [keyword, page, filters]);

  return { data, loading, error };
}

// Usage in component
function SearchComponent() {
  const [keyword, setKeyword] = useState('');
  const [page, setPage] = useState(1);
  
  const { data, loading, error } = useSearchPapers(keyword, page);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div>
      <input 
        value={keyword} 
        onChange={(e) => setKeyword(e.target.value)} 
        placeholder="Search papers..."
      />
      
      {data?.success && (
        <div>
          <p>Found {data.data.total} results</p>
          {data.data.results.map(paper => (
            <div key={paper.paperId}>
              <h3>{paper.title}</h3>
              <p>{paper.abstract}</p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
```

### **Response Format**

#### **Success Response (200)**
```json
{
  "success": true,
  "data": {
    "total": 150,
    "page": 1,
    "size": 10,
    "results": [
      {
        "paperId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "Deep Learning for Natural Language Processing",
        "abstract": "This paper presents a comprehensive study on...",
        "publicationYear": 2024,
        "citedByCount": 42,
        "highlight": {
          "title": ["<em>Deep</em> <em>Learning</em> for Natural Language Processing"],
          "abstract": [
            "This paper presents a comprehensive study on <em>deep</em> <em>learning</em>...",
            "...neural networks have shown promising results in <em>deep</em> <em>learning</em>..."
          ]
        }
      }
    ]
  },
  "errors": null
}
```

#### **Error Response (401 Unauthorized)**
```json
{
  "success": false,
  "data": null,
  "errors": ["Unauthorized. Please login."]
}
```

#### **Error Response (400 Bad Request)**
```json
{
  "success": false,
  "data": null,
  "errors": ["Invalid page number"]
}
```

### **Response Fields**

| Field | Type | Description |
|-------|------|-------------|
| `success` | boolean | API call thành công hay không |
| `data` | object | Dữ liệu kết quả search |
| `data.total` | integer | Tổng số kết quả tìm được |
| `data.page` | integer | Trang hiện tại |
| `data.size` | integer | Số kết quả mỗi trang |
| `data.results` | array | Mảng các paper |
| `results[].paperId` | guid | ID của paper |
| `results[].title` | string | Tiêu đề paper |
| `results[].abstract` | string | Tóm tắt paper |
| `results[].publicationYear` | integer | Năm xuất bản |
| `results[].citedByCount` | integer | Số lần được trích dẫn |
| `results[].highlight` | object | Highlighting từ khóa |
| `highlight.title` | array | Title với từ khóa được highlight |
| `highlight.abstract` | array | Abstract với từ khóa được highlight (mảng fragments) |

### **Highlighting**

Elasticsearch trả về các đoạn text có chứa từ khóa, với từ khóa được wrap trong tag `<em>`:

```javascript
// Example highlight
{
  "title": ["<em>Machine</em> <em>Learning</em> in Healthcare"],
  "abstract": [
    "...applications of <em>machine</em> <em>learning</em> in medical diagnosis...",
    "...using <em>machine</em> <em>learning</em> algorithms to predict..."
  ]
}
```

**Render trong HTML:**
```javascript
// Option 1: Render as HTML (be careful with XSS!)
<div dangerouslySetInnerHTML={{ __html: paper.highlight.title[0] }} />

// Option 2: Parse and render safely
function renderHighlight(text) {
  return text.replace(/<em>/g, '<mark>').replace(/<\/em>/g, '</mark>');
}
```

**CSS Styling:**
```css
em, mark {
  background-color: #ffeb3b;
  font-weight: bold;
  font-style: normal;
}
```

---

## 📊 2. Pagination

### **Calculate Total Pages**
```javascript
function calculateTotalPages(total, size) {
  return Math.ceil(total / size);
}

const totalPages = calculateTotalPages(data.data.total, data.data.size);
```

### **Pagination Component Example (React)**
```javascript
function Pagination({ currentPage, totalResults, pageSize, onPageChange }) {
  const totalPages = Math.ceil(totalResults / pageSize);
  
  return (
    <div className="pagination">
      <button 
        disabled={currentPage === 1}
        onClick={() => onPageChange(currentPage - 1)}
      >
        Previous
      </button>
      
      <span>Page {currentPage} of {totalPages}</span>
      
      <button 
        disabled={currentPage === totalPages}
        onClick={() => onPageChange(currentPage + 1)}
      >
        Next
      </button>
    </div>
  );
}
```

---

## 🎨 3. UI/UX Best Practices

### **Search Box with Debounce**
```javascript
import { useState, useEffect } from 'react';

function SearchBox({ onSearch }) {
  const [input, setInput] = useState('');

  useEffect(() => {
    // Debounce: chỉ search sau 500ms user ngừng gõ
    const timer = setTimeout(() => {
      if (input.length >= 3) {
        onSearch(input);
      }
    }, 500);

    return () => clearTimeout(timer);
  }, [input]);

  return (
    <input
      type="text"
      placeholder="Search papers..."
      value={input}
      onChange={(e) => setInput(e.target.value)}
      minLength={3}
    />
  );
}
```

### **Loading State**
```javascript
function SearchResults({ loading, data }) {
  if (loading) {
    return (
      <div className="loading">
        <div className="spinner"></div>
        <p>Searching...</p>
      </div>
    );
  }

  if (!data) {
    return <div>Enter keywords to search</div>;
  }

  return (
    <div>
      <p>{data.data.total} results found</p>
      {/* Render results */}
    </div>
  );
}
```

### **Empty State**
```javascript
function EmptyState() {
  return (
    <div className="empty-state">
      <img src="/no-results.svg" alt="No results" />
      <h3>No papers found</h3>
      <p>Try different keywords or adjust your filters</p>
    </div>
  );
}
```

### **Error Handling**
```javascript
function ErrorMessage({ error }) {
  if (!error) return null;
  
  return (
    <div className="error">
      <p>⚠️ {error}</p>
      <button onClick={() => window.location.reload()}>
        Retry
      </button>
    </div>
  );
}
```

---

## 🔧 4. Advanced Filters

### **Filter Component Example**
```javascript
function AdvancedFilters({ onFilterChange }) {
  const [filters, setFilters] = useState({
    fromYear: '',
    toYear: '',
    language: '',
    isOpenAccess: null
  });

  const handleChange = (key, value) => {
    const newFilters = { ...filters, [key]: value };
    setFilters(newFilters);
    onFilterChange(newFilters);
  };

  return (
    <div className="filters">
      <h4>Filters</h4>
      
      <div className="filter-group">
        <label>Year Range</label>
        <input
          type="number"
          placeholder="From"
          value={filters.fromYear}
          onChange={(e) => handleChange('fromYear', e.target.value)}
        />
        <input
          type="number"
          placeholder="To"
          value={filters.toYear}
          onChange={(e) => handleChange('toYear', e.target.value)}
        />
      </div>

      <div className="filter-group">
        <label>Language</label>
        <select
          value={filters.language}
          onChange={(e) => handleChange('language', e.target.value)}
        >
          <option value="">All</option>
          <option value="en">English</option>
          <option value="vi">Vietnamese</option>
        </select>
      </div>

      <div className="filter-group">
        <label>
          <input
            type="checkbox"
            checked={filters.isOpenAccess === true}
            onChange={(e) => handleChange('isOpenAccess', e.target.checked ? true : null)}
          />
          Open Access Only
        </label>
      </div>
    </div>
  );
}
```

---

## 🚨 5. Error Handling

### **Common Errors**

| HTTP Status | Meaning | Solution |
|-------------|---------|----------|
| 401 | Unauthorized | Token expired or invalid → Redirect to login |
| 400 | Bad Request | Invalid parameters → Show error message |
| 404 | Not Found | API endpoint wrong → Check URL |
| 500 | Server Error | Backend issue → Show retry button |

### **Error Handler**
```javascript
async function handleSearchError(error) {
  if (error.response) {
    // Server responded with error
    switch (error.response.status) {
      case 401:
        // Token expired
        localStorage.removeItem('jwt_token');
        window.location.href = '/login';
        break;
      
      case 400:
        alert('Invalid search parameters');
        break;
      
      case 500:
        alert('Server error. Please try again later.');
        break;
      
      default:
        alert('An error occurred');
    }
  } else if (error.request) {
    // Network error
    alert('Network error. Check your internet connection.');
  } else {
    alert('An unexpected error occurred');
  }
}
```

---

## 📱 6. Complete Example

### **React Search Component (Full Example)**

```javascript
import React, { useState, useEffect } from 'react';
import axios from 'axios';

const API_BASE_URL = 'http://localhost:5000/api';

function PaperSearch() {
  const [keyword, setKeyword] = useState('');
  const [results, setResults] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [page, setPage] = useState(1);
  const [filters, setFilters] = useState({
    fromYear: '',
    toYear: '',
    language: '',
    isOpenAccess: null
  });

  const searchPapers = async () => {
    if (!keyword || keyword.length < 3) return;

    setLoading(true);
    setError(null);

    try {
      const token = localStorage.getItem('jwt_token');
      const params = {
        q: keyword,
        page: page,
        size: 10,
        ...(filters.fromYear && { from: filters.fromYear }),
        ...(filters.toYear && { to: filters.toYear }),
        ...(filters.language && { language: filters.language }),
        ...(filters.isOpenAccess !== null && { isOpenAccess: filters.isOpenAccess })
      };

      const response = await axios.get(`${API_BASE_URL}/search/papers`, {
        params,
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      setResults(response.data.data);
    } catch (err) {
      if (err.response?.status === 401) {
        localStorage.removeItem('jwt_token');
        window.location.href = '/login';
      } else {
        setError(err.message);
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const timer = setTimeout(() => {
      searchPapers();
    }, 500);

    return () => clearTimeout(timer);
  }, [keyword, page, filters]);

  const renderHighlight = (text) => {
    if (!text) return '';
    return { __html: text };
  };

  return (
    <div className="paper-search">
      <div className="search-header">
        <input
          type="text"
          placeholder="Search papers (min 3 characters)..."
          value={keyword}
          onChange={(e) => {
            setKeyword(e.target.value);
            setPage(1); // Reset to page 1 on new search
          }}
          className="search-input"
        />
      </div>

      <div className="search-filters">
        <input
          type="number"
          placeholder="From Year"
          value={filters.fromYear}
          onChange={(e) => setFilters({ ...filters, fromYear: e.target.value })}
        />
        <input
          type="number"
          placeholder="To Year"
          value={filters.toYear}
          onChange={(e) => setFilters({ ...filters, toYear: e.target.value })}
        />
        <select
          value={filters.language}
          onChange={(e) => setFilters({ ...filters, language: e.target.value })}
        >
          <option value="">All Languages</option>
          <option value="en">English</option>
          <option value="vi">Vietnamese</option>
        </select>
        <label>
          <input
            type="checkbox"
            checked={filters.isOpenAccess === true}
            onChange={(e) => setFilters({ 
              ...filters, 
              isOpenAccess: e.target.checked ? true : null 
            })}
          />
          Open Access Only
        </label>
      </div>

      {loading && (
        <div className="loading">
          <div className="spinner"></div>
          <p>Searching...</p>
        </div>
      )}

      {error && (
        <div className="error">
          <p>⚠️ Error: {error}</p>
          <button onClick={searchPapers}>Retry</button>
        </div>
      )}

      {results && !loading && (
        <div className="search-results">
          <div className="results-header">
            <p>{results.total} papers found</p>
          </div>

          {results.results.length === 0 ? (
            <div className="empty-state">
              <p>No papers found. Try different keywords.</p>
            </div>
          ) : (
            <div className="papers-list">
              {results.results.map((paper) => (
                <div key={paper.paperId} className="paper-card">
                  <h3 
                    dangerouslySetInnerHTML={
                      renderHighlight(paper.highlight.title[0] || paper.title)
                    }
                  />
                  <div className="paper-meta">
                    <span>Year: {paper.publicationYear}</span>
                    <span>Citations: {paper.citedByCount}</span>
                  </div>
                  <p className="abstract">
                    {paper.highlight.abstract.length > 0 ? (
                      <span 
                        dangerouslySetInnerHTML={
                          renderHighlight(paper.highlight.abstract[0])
                        }
                      />
                    ) : (
                      paper.abstract?.substring(0, 300) + '...'
                    )}
                  </p>
                </div>
              ))}
            </div>
          )}

          {results.total > results.size && (
            <div className="pagination">
              <button
                disabled={page === 1}
                onClick={() => setPage(page - 1)}
              >
                Previous
              </button>
              <span>
                Page {page} of {Math.ceil(results.total / results.size)}
              </span>
              <button
                disabled={page >= Math.ceil(results.total / results.size)}
                onClick={() => setPage(page + 1)}
              >
                Next
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default PaperSearch;
```

### **CSS Example**
```css
.paper-search {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.search-input {
  width: 100%;
  padding: 15px;
  font-size: 16px;
  border: 2px solid #ddd;
  border-radius: 8px;
  margin-bottom: 20px;
}

.search-filters {
  display: flex;
  gap: 10px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}

.search-filters input,
.search-filters select {
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.loading {
  text-align: center;
  padding: 40px;
}

.spinner {
  border: 4px solid #f3f3f3;
  border-top: 4px solid #3498db;
  border-radius: 50%;
  width: 40px;
  height: 40px;
  animation: spin 1s linear infinite;
  margin: 0 auto;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.paper-card {
  background: white;
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 20px;
  margin-bottom: 20px;
}

.paper-card h3 {
  margin-top: 0;
  color: #2c3e50;
}

.paper-card em {
  background-color: #ffeb3b;
  font-style: normal;
  font-weight: bold;
  padding: 2px 4px;
  border-radius: 3px;
}

.paper-meta {
  display: flex;
  gap: 20px;
  color: #666;
  font-size: 14px;
  margin: 10px 0;
}

.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 20px;
  margin-top: 30px;
}

.pagination button {
  padding: 10px 20px;
  background: #3498db;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.pagination button:disabled {
  background: #ccc;
  cursor: not-allowed;
}
```

---

## 🎯 7. Performance Tips

### **1. Debounce Search Input**
```javascript
import { debounce } from 'lodash';

const debouncedSearch = debounce((keyword) => {
  searchPapers(keyword);
}, 500);
```

### **2. Cache Results**
```javascript
const cache = new Map();

async function searchWithCache(keyword, page) {
  const cacheKey = `${keyword}-${page}`;
  
  if (cache.has(cacheKey)) {
    return cache.get(cacheKey);
  }
  
  const result = await searchPapers(keyword, page);
  cache.set(cacheKey, result);
  
  return result;
}
```

### **3. Limit Results per Page**
- Default: 10 results/page
- Max recommended: 50 results/page
- Quá nhiều sẽ làm chậm UI

---

## 🔍 7. Search Authors

### **Endpoint**
```
GET /api/search/authors
```

### **Description**
Full-text search tác giả bằng Elasticsearch, hỗ trợ highlighting, pagination và các bộ lọc số lượng bài báo, số lượt trích dẫn, chỉ số H-Index.

### **Authorization**
✅ Required (User hoặc Admin)

### **Query Parameters**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `q` | string | ❌ | - | Từ khóa tìm kiếm (search trong displayName, fullName, affiliations, lastKnownInstitutions) |
| `page` | integer | ❌ | 1 | Số trang (pagination) |
| `size` | integer | ❌ | 10 | Số kết quả mỗi trang (max: 100) |
| `minWorksCount` | integer | ❌ | - | Số lượng bài viết tối thiểu của tác giả |
| `maxWorksCount` | integer | ❌ | - | Số lượng bài viết tối đa của tác giả |
| `minCitedByCount` | integer | ❌ | - | Số lượt trích dẫn tối thiểu của tác giả |
| `maxCitedByCount` | integer | ❌ | - | Số lượt trích dẫn tối đa của tác giả |
| `minHIndex` | integer | ❌ | - | Chỉ số H-Index tối thiểu |
| `maxHIndex` | integer | ❌ | - | Chỉ số H-Index tối đa |

### **Request Examples**

#### **JavaScript (Fetch)**
```javascript
async function searchAuthors(keyword, filters = {}) {
  const token = localStorage.getItem('jwt_token');
  const params = new URLSearchParams({
    q: keyword,
    page: filters.page || 1,
    size: filters.size || 10,
    ...(filters.minWorksCount && { minWorksCount: filters.minWorksCount }),
    ...(filters.maxWorksCount && { maxWorksCount: filters.maxWorksCount }),
    ...(filters.minCitedByCount && { minCitedByCount: filters.minCitedByCount }),
    ...(filters.maxCitedByCount && { maxCitedByCount: filters.maxCitedByCount }),
    ...(filters.minHIndex && { minHIndex: filters.minHIndex }),
    ...(filters.maxHIndex && { maxHIndex: filters.maxHIndex })
  });

  const response = await fetch(
    `http://localhost:5000/api/search/authors?${params}`,
    {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    }
  );
  
  return await response.json();
}
```

### **Response Format**

#### **Success Response (200)**
```json
{
  "success": true,
  "data": {
    "total": 12,
    "page": 1,
    "size": 10,
    "results": [
      {
        "authorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "displayName": "John Doe",
        "fullName": "John Jonathan Doe",
        "orcid": "https://orcid.org/0000-0002-1825-0097",
        "worksCount": 45,
        "citedByCount": 350,
        "hIndex": 12,
        "affiliations": "Department of Computer Science, Stanford University",
        "lastKnownInstitutions": "Stanford University",
        "highlight": {
          "displayName": ["<em>John</em> Doe"],
          "fullName": ["<em>John</em> Jonathan Doe"],
          "affiliations": []
        }
      }
    ]
  },
  "errors": null
}
```

### **Response Fields**

| Field | Type | Description |
|-------|------|-------------|
| `success` | boolean | Trạng thái thành công |
| `data` | object | Dữ liệu kết quả tìm kiếm |
| `data.total` | integer | Tổng số tác giả tìm được |
| `data.page` | integer | Trang hiện tại |
| `data.size` | integer | Số phần tử mỗi trang |
| `data.results` | array | Mảng danh sách tác giả khớp |
| `results[].authorId` | guid | ID tác giả |
| `results[].displayName` | string | Tên hiển thị |
| `results[].fullName` | string | Tên đầy đủ |
| `results[].orcid` | string | ORCID link/id |
| `results[].worksCount` | integer | Tổng số công trình/bài báo đã xuất bản |
| `results[].citedByCount` | integer | Tổng số lượt trích dẫn |
| `results[].hIndex` | integer | Chỉ số H-Index |
| `results[].affiliations` | string | Đơn vị công tác (Affiliation) |
| `results[].lastKnownInstitutions` | string | Tổ chức công tác gần nhất |
| `results[].highlight` | object | Các trường khớp được highlight (displayName, fullName, affiliations) |

---

## 📞 8. Support & Contact

- **Backend Team**: [Slack Channel]
- **API Issues**: [GitHub Issues]
- **Documentation**: [Confluence]

---

## ✅ Checklist cho Frontend Dev

- [ ] Setup Axios/Fetch với base URL
- [ ] Implement JWT token authentication
- [ ] Create search input với debounce
- [ ] Handle loading state
- [ ] Handle error state (401, 400, 500)
- [ ] Implement pagination
- [ ] Render highlighting (với `<em>` tags)
- [ ] Add filters (year, language, open access)
- [ ] Test với nhiều keywords khác nhau
- [ ] Handle empty results
- [ ] Responsive design cho mobile

---

**Happy Coding! 🚀**
