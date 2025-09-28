<template>
  <div class="max-w-3xl mx-auto px-4 py-8 flex-1 w-full flex flex-col">
    <div class="space-y-6 flex-1 flex flex-col">
      <div class="bg-white/80 backdrop-blur border-2 border-primary-from rounded-lg p-6 space-y-4">
        <h1 class="text-2xl font-bold text-gray-900">How to export your ChatGPT history</h1>
        <ol class="list-decimal list-inside text-left text-gray-700 space-y-1">
          <li>Open ChatGPT and navigate to <strong>Settings &gt; Data Controls</strong>.</li>
          <li>Select <strong>Export</strong> and confirm to request your data.</li>
          <li>When you receive the email, download the provided ZIP file.</li>
          <li>Upload that ZIP using the form below.</li>
        </ol>
        <p class="text-sm text-gray-500">All files are anonymized before becoming part of the public corpus.</p>
      </div>
      <div
        class="border-2 border-dashed border-gray-200 rounded-lg p-8 text-center bg-gray-50/50 hover:border-primary-from transition-colors duration-200 w-full"
        :class="{ 'border-primary-from bg-primary-from/5 shadow-[0_0_15px_rgba(168,85,247,0.3)]': isDragging }"
        @dragenter.prevent="isDragging = true"
        @dragleave.prevent="isDragging = false"
        @dragover.prevent
        @drop.prevent="handleFileDrop"
      >
        <div class="space-y-4">
          <div class="text-gray-500">
            <svg
              class="mx-auto h-12 w-12"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"
              />
            </svg>
            <p class="mt-2">Drag and drop your ZIP file here</p>
            <p class="text-sm">or</p>
          </div>
          <label class="btn-primary inline-block cursor-pointer">
            <span>Choose File</span>
            <input
              type="file"
              accept=".zip"
              class="hidden"
              @change="handleFileSelect"
            />
          </label>
        </div>
      </div>

      <div v-if="selectedFile" class="card p-6 bg-white rounded-xl shadow-sm border border-gray-100">
        <div class="flex items-center justify-between mb-6">
          <div class="flex items-center space-x-4">
            <div class="p-2 bg-primary-from/10 rounded-lg">
              <svg class="h-8 w-8 text-primary-from" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4" />
              </svg>
            </div>
            <div>
              <p class="font-medium text-gray-900 text-lg">{{ selectedFile.name }}</p>
              <p class="text-sm text-gray-500">{{ formatFileSize(selectedFile.size) }}</p>
            </div>
          </div>
        </div>

        <!-- Metadata Form -->
        <div class="space-y-6">
          <div class="bg-gray-50/50 rounded-lg p-6 border border-gray-100 w-full">
            <h3 class="text-lg font-medium text-gray-900 mb-4">Usage Information</h3>
            <div class="space-y-4 w-full">
              <div class="space-y-2">
                <label class="block text-sm font-medium text-gray-700">Usage Type</label>
                <div class="flex items-center space-x-6">
                  <label class="relative flex items-center cursor-pointer">
                    <input
                      type="radio"
                      v-model="metadata.isSingleUser"
                      :value="true"
                      class="peer sr-only"
                    />
                    <div class="w-5 h-5 border-2 border-gray-300 rounded-full peer-checked:border-primary-from peer-checked:bg-primary-from/10 flex items-center justify-center">
                      <div class="w-2.5 h-2.5 rounded-full bg-primary-from transform scale-0 peer-checked:scale-100 transition-transform duration-200"></div>
                    </div>
                    <span class="ml-2 text-gray-700">Single User</span>
                  </label>
                  <label class="relative flex items-center cursor-pointer">
                    <input
                      type="radio"
                      v-model="metadata.isSingleUser"
                      :value="false"
                      class="peer sr-only"
                    />
                    <div class="w-5 h-5 border-2 border-gray-300 rounded-full peer-checked:border-primary-from peer-checked:bg-primary-from/10 flex items-center justify-center">
                      <div class="w-2.5 h-2.5 rounded-full bg-primary-from transform scale-0 peer-checked:scale-100 transition-transform duration-200"></div>
                    </div>
                    <span class="ml-2 text-gray-700">Multiple Users</span>
                  </label>
                </div>
              </div>
            </div>
          </div>

          <!-- Single User Additional Fields -->
          <div v-if="metadata.isSingleUser" class="bg-gray-50/50 rounded-lg p-6 border border-gray-100 w-full">
            <h3 class="text-lg font-medium text-gray-900 mb-4">Demographic Information</h3>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6 w-full">
              <div class="space-y-2">
                <label class="block text-sm font-medium text-gray-700">Gender</label>
                <select
                  v-model="metadata.gender"
                  class="input w-full bg-white/80 backdrop-blur border-2 border-primary-from focus:border-primary-to shadow-[0_0_8px_rgba(168,85,247,0.08)]"
                >
                  <option value="">Select gender</option>
                  <option value="male">Male</option>
                  <option value="female">Female</option>
                  <option value="other">Other</option>
                  <option value="prefer_not_to_say">Prefer not to say</option>
                </select>
              </div>

              <div class="space-y-2">
                <label class="block text-sm font-medium text-gray-700">Age Category</label>
                <select
                  v-model="metadata.ageCategory"
                  class="input w-full bg-white/80 backdrop-blur border-2 border-primary-from focus:border-primary-to shadow-[0_0_8px_rgba(168,85,247,0.08)]"
                >
                  <option value="">Select age category</option>
                  <option value="under_18">Under 18</option>
                  <option value="18_24">18-24</option>
                  <option value="25_34">25-34</option>
                  <option value="35_44">35-44</option>
                  <option value="45_54">45-54</option>
                  <option value="55_64">55-64</option>
                  <option value="65_plus">65+</option>
                  <option value="prefer_not_to_say">Prefer not to say</option>
                </select>
              </div>

              <div class="space-y-2">
                <label class="block text-sm font-medium text-gray-700">Education Level</label>
                <select
                  v-model="metadata.educationLevel"
                  class="input w-full bg-white/80 backdrop-blur border-2 border-primary-from focus:border-primary-to shadow-[0_0_8px_rgba(168,85,247,0.08)]"
                >
                  <option value="">Select education level</option>
                  <option value="primary">Primary School</option>
                  <option value="secondary">Secondary School</option>
                  <option value="high_school">High School</option>
                  <option value="bachelors">Bachelor's Degree</option>
                  <option value="masters">Master's Degree</option>
                  <option value="phd">PhD</option>
                  <option value="other">Other</option>
                  <option value="prefer_not_to_say">Prefer not to say</option>
                </select>
              </div>

              <div class="space-y-2">
                <label class="block text-sm font-medium text-gray-700">Current Region</label>
                <input
                  type="text"
                  v-model="metadata.currentRegion"
                  placeholder="Enter your current region"
                  class="input w-full bg-white/80 backdrop-blur border-2 border-primary-from focus:border-primary-to shadow-[0_0_8px_rgba(168,85,247,0.08)]"
                />
              </div>

              <div class="space-y-2">
                <label class="block text-sm font-medium text-gray-700">Childhood Region</label>
                <input
                  type="text"
                  v-model="metadata.childhoodRegion"
                  placeholder="Enter your childhood region"
                  class="input w-full bg-white/80 backdrop-blur border-2 border-primary-from focus:border-primary-to shadow-[0_0_8px_rgba(168,85,247,0.08)]"
                />
              </div>
            </div>
          </div>

          <div class="flex justify-end">
            <button
              @click="uploadFile"
              class="btn-primary px-6 py-2.5 text-sm font-medium rounded-lg shadow-sm hover:shadow-md transition-all duration-200"
              :disabled="isUploading"
            >
              <span v-if="isUploading" class="flex items-center">
                <svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Uploading...
              </span>
              <span v-else>Upload</span>
            </button>
          </div>
        </div>

        <div v-if="isUploading" class="mt-6">
          <div class="w-full bg-gray-100 rounded-full h-2 overflow-hidden">
            <div
              class="h-2 rounded-full bg-gradient-to-r from-primary-from to-primary-to transition-all duration-200"
              :style="{ width: uploadProgress + '%' }"
            ></div>
          </div>
          <div class="text-xs text-gray-500 mt-2 text-right">{{ uploadProgress }}%</div>
        </div>
      </div>

      <div v-if="uploadStatus" class="mt-4">
        <div
          :class="[
            'p-4 rounded-lg text-center',
            uploadStatus.type === 'success' 
              ? 'bg-green-50 text-green-700 border border-green-200' 
              : 'bg-red-50 text-red-700 border border-red-200'
          ]"
        >
          {{ uploadStatus.message }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { uploadZip } from '../services/uploadService'

const isDragging = ref(false)
const selectedFile = ref(null)
const isUploading = ref(false)
const uploadStatus = ref(null)
const uploadProgress = ref(0)
const metadata = ref({
  isSingleUser: true,
  gender: '',
  ageCategory: '',
  educationLevel: '',
  currentRegion: '',
  childhoodRegion: ''
})

const handleFileDrop = (event) => {
  isDragging.value = false
  const file = event.dataTransfer.files[0]
  if (file && file.name.endsWith('.zip')) {
    selectedFile.value = file
  }
}

const handleFileSelect = (event) => {
  const file = event.target.files[0]
  if (file && file.name.endsWith('.zip')) {
    selectedFile.value = file
  }
}

const uploadFile = async () => {
  if (!selectedFile.value) return

  isUploading.value = true
  uploadStatus.value = null
  uploadProgress.value = 0

  const result = await uploadZip(selectedFile.value, (percent) => {
    uploadProgress.value = percent
  }, metadata.value)
  
  if (result.success) {
    uploadStatus.value = {
      type: 'success',
      message: 'File uploaded successfully!'
    }
    selectedFile.value = null
    // Reset metadata
    metadata.value = {
      isSingleUser: true,
      gender: '',
      ageCategory: '',
      educationLevel: '',
      currentRegion: '',
      childhoodRegion: ''
    }
  } else {
    uploadStatus.value = {
      type: 'error',
      message: result.error || 'Failed to upload file. Please try again.'
    }
  }

  isUploading.value = false
  uploadProgress.value = 0
}

const formatFileSize = (bytes) => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}
</script> 
