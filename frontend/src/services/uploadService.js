const API_BASE_URL = '/api';

export function uploadZip(file, onProgress, metadata = null) {
  return new Promise((resolve, reject) => {
    const formData = new FormData();
    formData.append('file', file);
    
    if (metadata) {
      formData.append('metadata', JSON.stringify(metadata));
    }

    const xhr = new XMLHttpRequest();
    xhr.open('POST', `${API_BASE_URL}/upload`);

    xhr.upload.onprogress = (event) => {
      if (event.lengthComputable && typeof onProgress === 'function') {
        const percent = Math.round((event.loaded / event.total) * 100);
        onProgress(percent);
      }
    };

    xhr.onload = () => {
      if (xhr.status >= 200 && xhr.status < 300) {
        try {
          const data = JSON.parse(xhr.responseText);
          resolve({ success: true, data, error: null });
        } catch (e) {
          resolve({ success: true, data: null, error: null });
        }
      } else {
        let errorMsg = `HTTP error! status: ${xhr.status}`;
        try {
          const errorData = JSON.parse(xhr.responseText);
          errorMsg = errorData.error || errorMsg;
        } catch {}
        resolve({ success: false, data: null, error: errorMsg });
      }
    };

    xhr.onerror = () => {
      resolve({ success: false, data: null, error: 'Network error' });
    };

    xhr.send(formData);
  });
} 