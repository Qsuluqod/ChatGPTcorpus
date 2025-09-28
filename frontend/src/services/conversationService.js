import { api } from './api'

export async function getConversation(conversationId, options = {}) {
  try {
    const params = {}
    if (options.messageSequence !== undefined && options.messageSequence !== null) {
      params.messageSequence = options.messageSequence
    }

    const response = await api.get(`/conversations/${conversationId}`, { params })
    return {
      success: true,
      conversation: response.data
    }
  } catch (error) {
    console.error('Error fetching conversation:', error)
    return {
      success: false,
      error: 'Failed to load conversation'
    }
  }
} 
