# Search API Examples

This document provides practical examples for using the paper search API with Elasticsearch.

## Authentication

All search endpoints require authentication. First, obtain a JWT token:

```bash
# Login
curl -X POST "http://localhost:8080/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "your_password"
  }'

# Response
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "email": "user@example.com"
  }
}
```

Use the token in subsequent requests:

```bash
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Basic Search Examples

### 1. Simple Keyword Search

Search for papers containing "machine learning":

```bash
curl -X GET "http://localhost:8080/api/search/papers?q=machine%20learning" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Response:**
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
        "title": "Deep Machine Learning Techniques",
        "abstract": "This paper explores various machine learning algorithms...",
        "publicationYear": 2024,
        "citedByCount": 42,
        "highlight": {
          "title": ["Deep <em>Machine Learning</em> Techniques"],
          "abstract": ["explores various <em>machine learning</em> algorithms"]
        }
      }
    ]
  }
}
```

### 2. Search with Pagination

Get page 2 with 20 results per page:

```bash
curl -X GET "http://localhost:8080/api/search/papers?q=neural%20networks&page=2&size=20" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 3. Search with Year Range

Find papers published between 2020 and 2023:

```bash
curl -X GET "http://localhost:8080/api/search/papers?q=artificial%20intelligence&from=2020&to=2023" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 4. Filter by Language

Search for papers in English:

```bash
curl -X GET "http://localhost:8080/api/search/papers?q=deep%20learning&language=en" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 5. Filter Open Access Papers

Find only open access papers:

```bash
curl -X GET "http://localhost:8080/api/search/papers?q=covid-19&isOpenAccess=true" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Advanced Search Examples

### 6. Combined Filters

Search with multiple filters:

```bash
curl -X GET "http://localhost:8080/api/search/papers?q=quantum%20computing&from=2020&to=2024&language=en&isOpenAccess=true&page=1&size=10" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 7. Empty Query (Get All Papers)

Retrieve all papers with filters only:

```bash
curl -X GET "http://localhost:8080/api/search/papers?from=2023&to=2024&isOpenAccess=true" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 8. Recent High-Impact Papers

Find highly cited recent papers:

```bash
# Note: Sorting by citation count requires custom implementation
curl -X GET "http://localhost:8080/api/search/papers?q=&from=2023&size=50" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## JavaScript/TypeScript Examples

### Using Fetch API

```javascript
const searchPapers = async (query, filters = {}) => {
  const token = localStorage.getItem('jwt_token');
  
  const params = new URLSearchParams({
    q: query,
    page: filters.page || 1,
    size: filters.size || 10,
    ...(filters.from && { from: filters.from }),
    ...(filters.to && { to: filters.to }),
    ...(filters.language && { language: filters.language }),
    ...(filters.isOpenAccess !== undefined && { isOpenAccess: filters.isOpenAccess })
  });

  try {
    const response = await fetch(
      `http://localhost:8080/api/search/papers?${params}`,
      {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      }
    );
    
    const data = await response.json();
    return data.data; // Returns SearchPaperResponseModel
  } catch (error) {
    console.error('Search failed:', error);
    throw error;
  }
};

// Usage
searchPapers('machine learning', {
  from: 2020,
  to: 2024,
  isOpenAccess: true,
  page: 1,
  size: 20
}).then(results => {
  console.log(`Found ${results.total} papers`);
  results.results.forEach(paper => {
    console.log(`- ${paper.title} (${paper.publicationYear})`);
  });
});
```

### Using Axios

```typescript
import axios from 'axios';

interface SearchFilters {
  page?: number;
  size?: number;
  from?: number;
  to?: number;
  language?: string;
  isOpenAccess?: boolean;
}

interface SearchResult {
  paperId: string;
  title: string;
  abstract: string;
  publicationYear?: number;
  citedByCount?: number;
  highlight: {
    title: string[];
    abstract: string[];
  };
}

interface SearchResponse {
  total: number;
  page: number;
  size: number;
  results: SearchResult[];
}

const searchPapers = async (
  query: string, 
  filters?: SearchFilters
): Promise<SearchResponse> => {
  const token = localStorage.getItem('jwt_token');
  
  const response = await axios.get<{ success: boolean; data: SearchResponse }>(
    'http://localhost:8080/api/search/papers',
    {
      params: {
        q: query,
        ...filters
      },
      headers: {
        'Authorization': `Bearer ${token}`
      }
    }
  );
  
  return response.data.data;
};

// Usage
const results = await searchPapers('artificial intelligence', {
  from: 2020,
  to: 2024,
  isOpenAccess: true,
  page: 1,
  size: 10
});
```

## React Hook Example

```typescript
import { useState, useEffect } from 'react';
import axios from 'axios';

const useSearchPapers = () => {
  const [results, setResults] = useState<SearchResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const search = async (query: string, filters?: SearchFilters) => {
    setLoading(true);
    setError(null);
    
    try {
      const token = localStorage.getItem('jwt_token');
      const response = await axios.get(
        'http://localhost:8080/api/search/papers',
        {
          params: { q: query, ...filters },
          headers: { 'Authorization': `Bearer ${token}` }
        }
      );
      
      setResults(response.data.data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return { results, loading, error, search };
};

// Component usage
const SearchPage = () => {
  const { results, loading, error, search } = useSearchPapers();
  const [query, setQuery] = useState('');

  const handleSearch = () => {
    search(query, { isOpenAccess: true });
  };

  return (
    <div>
      <input 
        value={query} 
        onChange={(e) => setQuery(e.target.value)} 
      />
      <button onClick={handleSearch}>Search</button>
      
      {loading && <p>Loading...</p>}
      {error && <p>Error: {error}</p>}
      
      {results && (
        <div>
          <p>Found {results.total} papers</p>
          {results.results.map(paper => (
            <div key={paper.paperId}>
              <h3 dangerouslySetInnerHTML={{ 
                __html: paper.highlight.title[0] || paper.title 
              }} />
              <p>{paper.publicationYear} • Citations: {paper.citedByCount}</p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
```

## Python Example

```python
import requests
from typing import Optional, Dict, Any

class PaperSearchClient:
    def __init__(self, base_url: str, token: str):
        self.base_url = base_url
        self.headers = {'Authorization': f'Bearer {token}'}
    
    def search_papers(
        self,
        query: str,
        page: int = 1,
        size: int = 10,
        from_year: Optional[int] = None,
        to_year: Optional[int] = None,
        language: Optional[str] = None,
        is_open_access: Optional[bool] = None
    ) -> Dict[str, Any]:
        params = {
            'q': query,
            'page': page,
            'size': size
        }
        
        if from_year:
            params['from'] = from_year
        if to_year:
            params['to'] = to_year
        if language:
            params['language'] = language
        if is_open_access is not None:
            params['isOpenAccess'] = is_open_access
        
        response = requests.get(
            f'{self.base_url}/api/search/papers',
            params=params,
            headers=self.headers
        )
        response.raise_for_status()
        
        return response.json()['data']

# Usage
client = PaperSearchClient(
    base_url='http://localhost:8080',
    token='your_jwt_token_here'
)

results = client.search_papers(
    query='machine learning',
    from_year=2020,
    to_year=2024,
    is_open_access=True,
    page=1,
    size=20
)

print(f"Found {results['total']} papers")
for paper in results['results']:
    print(f"- {paper['title']} ({paper['publicationYear']})")
```

## Admin Operations

### Trigger Manual Reindex

```bash
curl -X POST "http://localhost:8080/api/admin/reindex" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

**Response:**
```json
{
  "success": true,
  "data": {
    "jobId": "12345",
    "message": "Reindex job queued successfully"
  }
}
```

### Create Elasticsearch Index

```bash
curl -X POST "http://localhost:8080/api/admin/elasticsearch/create-index" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

### Clear Search Cache

```bash
curl -X POST "http://localhost:8080/api/admin/cache/clear" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

## Postman Collection

Import this collection into Postman:

```json
{
  "info": {
    "name": "Paper Search API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Search Papers",
      "request": {
        "method": "GET",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{jwt_token}}"
          }
        ],
        "url": {
          "raw": "{{base_url}}/api/search/papers?q=machine learning&page=1&size=10&from=2020&to=2024&isOpenAccess=true",
          "host": ["{{base_url}}"],
          "path": ["api", "search", "papers"],
          "query": [
            {"key": "q", "value": "machine learning"},
            {"key": "page", "value": "1"},
            {"key": "size", "value": "10"},
            {"key": "from", "value": "2020"},
            {"key": "to", "value": "2024"},
            {"key": "isOpenAccess", "value": "true"}
          ]
        }
      }
    }
  ],
  "variable": [
    {
      "key": "base_url",
      "value": "http://localhost:8080"
    },
    {
      "key": "jwt_token",
      "value": "your_token_here"
    }
  ]
}
```

## Performance Tips

1. **Use Pagination Wisely**: Don't request too many results at once. Stick to 10-50 items per page.

2. **Cache on Client**: Cache search results on the client side for repeated queries.

3. **Debounce Search Input**: For search-as-you-type, debounce user input to reduce API calls.

```javascript
const debounce = (func, delay) => {
  let timeoutId;
  return (...args) => {
    clearTimeout(timeoutId);
    timeoutId = setTimeout(() => func(...args), delay);
  };
};

const debouncedSearch = debounce((query) => {
  searchPapers(query);
}, 300);
```

4. **Monitor Cache Hit Rate**: Check response times to verify caching is working.

5. **Use Specific Queries**: More specific queries return faster and more relevant results.
