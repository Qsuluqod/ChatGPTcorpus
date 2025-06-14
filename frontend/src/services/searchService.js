const API_BASE_URL = '/api';

export async function searchConversations(query) {
  try {
    const passphrase = localStorage.getItem('accessPassphrase') || ''
    const response = await fetch(`${API_BASE_URL}/search?q=${encodeURIComponent(query)}`, {
      headers: {
        'X-Access-Passphrase': passphrase
      }
    });
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    const data = await response.json();
    return {
      success: true,
      results: data.results || [],
      stats: data.stats || null,
      error: null
    };
  } catch (error) {
    console.error('Search failed:', error);
    return {
      success: false,
      results: [],
      stats: null,
      error: error.message || 'Failed to search conversations'
    };
  }
} 