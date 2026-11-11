<template>
  <view class="page">
    <view class="kicker">设置 / 关于</view>
    <view class="title">设置</view>
    <view class="subtitle">手机端本地优先；项目、分析、方案都保存在 App Storage，不需要登录。</view>

    <view class="card">
      <view class="section-title">存储</view>
      <view class="setting-line"><text class="muted">项目数量</text><text>{{ projectCount }}</text></view>
      <view class="setting-line"><text class="muted">Storage Key</text><text>{{ storage.keys.length }} 个</text></view>
      <view class="setting-line"><text class="muted">已用空间</text><text>{{ storage.currentSize }} KB / {{ limitText }}</text></view>
      <view class="btn-row">
        <button class="btn-secondary" @tap="clearAll">清空全部项目</button>
      </view>
    </view>

    <view class="card">
      <view class="section-title">PC 与手机</view>
      <view class="muted">手机可独立完成房间档案、声学分析、摆位/声装方案和参数卡；需要 AI 解释或 REW CSV 导入时，在 PC 端打开项目。</view>
      <view class="small" style="margin-top: 12rpx;">PC 数据目录：F:\RoomTuneData。手机运行时数据在 App 内，开发源码与构建产物统一放 F 盘。</view>
    </view>

    <view class="card">
      <view class="section-title">版本</view>
      <view class="setting-line"><text class="muted">应用</text><text>RoomTune 声居 1.0.0</text></view>
      <view class="setting-line"><text class="muted">平台</text><text>HBuilderX · uni-app Vue3 · Android</text></view>
      <view class="setting-line"><text class="muted">边界</text><text>不做实时 DSP，不控制 AVR</text></view>
    </view>
    <view class="small" style="text-align: center; margin-top: 28rpx;">RoomTune · 数字房间声学校正系统</view>
  </view>
</template>

<script>
import { loadProjects, getStorageInfo, clearAllProjects } from '../../utils/storage.js'

export default {
  data () {
    return {
      projectCount: 0,
      storage: { keys: [], currentSize: 0, limitSize: 0 }
    }
  },
  computed: {
    limitText () {
      const value = Number(this.storage.limitSize || 0)
      return value > 1000000 ? '—' : `${value} KB`
    }
  },
  onShow () {
    this.refresh()
  },
  methods: {
    refresh () {
      this.projectCount = loadProjects().length
      this.storage = getStorageInfo()
    },
    clearAll () {
      uni.showModal({
        title: '清空全部项目',
        content: '只清空手机端本机项目；PC 端数据不会被删除。确定继续？',
        success: result => {
          if (!result.confirm) return
          try {
            clearAllProjects()
            this.refresh()
            uni.showToast({ title: '已重置', icon: 'none' })
          } catch (error) {
            uni.showModal({ title: '重置失败', content: error.message, showCancel: false })
          }
        }
      })
    }
  }
}
</script>

<style>
.setting-line {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12rpx;
  min-height: 62rpx;
  border-bottom: 1rpx solid #E2E8F0;
  font-size: 23rpx;
  color: #1F2937;
}
.setting-line:last-of-type {
  border-bottom: none;
}
</style>
