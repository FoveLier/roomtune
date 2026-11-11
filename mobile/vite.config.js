import { defineConfig } from 'vite'
import uniModule from '@dcloudio/vite-plugin-uni'

const uni = uniModule && uniModule.default ? uniModule.default : uniModule

export default defineConfig({
  plugins: [uni()]
})
