const API_BASE_URL = '/api';

export async function searchConversations(query, options = {}) {
  try {
    const passphrase = localStorage.getItem('accessPassphrase') || ''
    const params = new URLSearchParams({ q: query })

    if (options.messageSequence !== undefined && options.messageSequence !== null && options.messageSequence !== '') {
      params.set('messageSequence', options.messageSequence)
    }

    if (options.messageSequenceMin !== undefined && options.messageSequenceMin !== null && options.messageSequenceMin !== '') {
      params.set('messageSequenceMin', options.messageSequenceMin)
    }

    if (options.messageSequenceMax !== undefined && options.messageSequenceMax !== null && options.messageSequenceMax !== '') {
      params.set('messageSequenceMax', options.messageSequenceMax)
    }

    if (options.maxPerImportBatch !== undefined && options.maxPerImportBatch !== null && options.maxPerImportBatch !== '') {
      params.set('maxPerImportBatch', options.maxPerImportBatch)
    }

    if (options.includeAll) {
      params.set('includeAll', 'true')
    }

    const response = await fetch(`${API_BASE_URL}/search?${params.toString()}`, {
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
