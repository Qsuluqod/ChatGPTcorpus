const API_BASE_URL = '/api';

export async function searchConversations(query) {
  try {
    const response = await fetch(`${API_BASE_URL}/search?q=${encodeURIComponent(query)}`);
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    const data = await response.json();
    return {
      success: true,
      results: data.results || [],
      error: null
    };
  } catch (error) {
    console.error('Search failed:', error);
    return {
      success: false,
      results: [],
      error: error.message || 'Failed to search conversations'
    };
  }
} 