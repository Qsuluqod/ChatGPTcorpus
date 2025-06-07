const API_BASE_URL = '/api';

export async function fetchCorpusStats() {
  try {
    const response = await fetch(`${API_BASE_URL}/search/stats`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const data = await response.json();
    return { success: true, stats: data, error: null };
  } catch (error) {
    console.error('Failed to fetch corpus stats:', error);
    return { success: false, stats: null, error: error.message || 'Failed to fetch corpus stats' };
  }
} 