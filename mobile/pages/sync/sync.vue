<template>
  <view class="page">
    <view class="kicker">同步 / 代码</view>
    <view class="title">同步码</view>
    <view class="subtitle">PC 和手机交换项目：导出 → 复制 → 在另一端粘贴导入。同步码只包含项目数据，不含 AI Key。</view>

    <view class="card">
      <view class="section-title">当前项目</view>
      <view class="muted">{{ projectName || '未选择项目' }}</view>
      <view class="btn-row">
        <button class="btn-primary" @tap="refreshCode">生成当前项目同步码</button>
        <button class="btn-secondary" @tap="copyCode">复制</button>
        <button class="btn-secondary" @tap="pasteCode">粘贴</button>
      </view>
    </view>

    <view class="card">
      <view class="section-title">同步码文本</view>
      <textarea v-model="code" class="code-area" placeholder="把 PC 端导出的同步码粘贴到这里，或点击上方生成当前项目同步码。"></textarea>
      <view class="btn-row">
        <button class="btn-secondary" style="flex: 1;" @tap="importCode">导入为新项目</button>
        <button class="btn-secondary" @tap="clearCode">清空</button>
      </view>
      <view class="small" style="margin-top: 14rpx;">导入会新建一个“（同步）”项目，不会覆盖手机上的现有数据。PC 端在“同步码”窗口粘贴后导入。</view>
    </view>

    <view class="status-text">{{ status }}</view>
  </view>
</template>

<script>
import { getCurrentProject, importProject } from '../../utils/storage.js'
import { encodeSync, decodeSync } from '../../utils/acoustics.js'

export default {
  data () {
    return {
      projectName: '',
      code: '',
      status: '先生成当前项目同步码，或粘贴 PC 端同步码。'
    }
  },
  onShow () {
    this.refreshCode()
  },
  methods: {
    refreshCode () {
      const project = getCurrentProject()
      if (!project) {
        this.projectName = ''
        this.code = ''
        return
      }
      this.projectName = project.Name
      this.code = encodeSync(project)
      this.status = '已生成；复制到 PC 的“同步码”窗口即可导入。'
    },
    copyCode () {
      if (!this.code) return
      uni.setClipboardData({
        data: this.code,
        success: () => { this.status = '同步码已复制。' }
      })
    },
    pasteCode () {
      uni.getClipboardData({
        success: result => {
          this.code = result.data || ''
          this.status = '已从剪贴板粘贴；点击“导入为新项目”。'
        },
        fail: () => { this.status = '读取剪贴板失败，请手动长按文本框粘贴。' }
      })
    },
    importCode () {
      try {
        const project = decodeSync(this.code)
        importProject(project)
        this.status = `已导入：${project.Name}。`
        uni.showToast({ title: '导入成功', icon: 'none' })
        setTimeout(() => {
          uni.switchTab({ url: '/pages/index/index' })
        }, 500)
      } catch (error) {
        this.status = `导入失败：${error.message}`
        uni.showModal({ title: '同步码无效', content: this.status, showCancel: false })
      }
    },
    clearCode () {
      this.code = ''
      this.status = '已清空文本框。'
    }
  }
}
</script>

<style>
.code-area {
  width: 100%;
  height: 360rpx;
  margin-top: 14rpx;
  padding: 16rpx;
  border: 1rpx solid #DCE2EA;
  border-radius: 10rpx;
  background-color: #F8FAFC;
  color: #334155;
  font-family: Consolas, monospace;
  font-size: 20rpx;
  line-height: 1.5;
}
.status-text {
  color: #2563EB;
  font-size: 21rpx;
  line-height: 1.6;
  margin-top: 16rpx;
}
</style>
