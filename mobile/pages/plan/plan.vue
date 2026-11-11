<template>
  <view class="page">
    <view class="kicker">校正 / 方案</view>
    <view class="title">校正方案</view>
    <view class="subtitle">先免费摆位，再软装/声装，最后才考虑 EQ；每条都写明取舍。</view>

    <view class="btn-row">
      <button class="btn-primary" style="flex: 1;" @tap="applyBalanced">应用对称摆位</button>
      <button class="btn-secondary" @tap="saveScheme">保存方案</button>
    </view>

    <view class="section-title" style="margin-top: 30rpx;">摆位方案</view>
    <view v-for="scheme in bundle.schemes" :key="scheme.name" class="card">
      <view class="scheme-head">
        <text class="scheme-name">{{ scheme.name }}</text>
        <text class="scheme-score">{{ scheme.score }} / 100</text>
      </view>
      <view class="muted">{{ scheme.summary }}</view>
      <view class="pros">优点：{{ scheme.pros.join('；') }}</view>
      <view class="cons">取舍：{{ scheme.cons.join('；') }}</view>
      <view v-for="(step, index) in scheme.steps" :key="index" class="step">{{ index + 1 }}. {{ step }}</view>
    </view>

    <view class="section-title" style="margin-top: 30rpx;">声学装修清单</view>
    <view v-for="item in bundle.treatments" :key="item.title" class="card">
      <view class="scheme-head">
        <text class="scheme-name">P{{ item.priority }} · {{ item.title }}</text>
        <text class="scheme-score">¥{{ item.budgetCny }}</text>
      </view>
      <view class="muted">{{ item.location }}</view>
      <view class="small">面积 {{ item.areaM2 }} m² · {{ item.thickness }} · {{ item.frequencyRange }}</view>
      <view class="pros">{{ item.effect }}</view>
      <view class="small">{{ item.note }}</view>
    </view>

    <view class="section-title" style="margin-top: 30rpx;">校准参数卡</view>
    <view class="card parameter-card">
      <text class="parameter-text">{{ bundle.parameters.text }}</text>
    </view>

    <view class="btn-row">
      <button class="btn-secondary" @tap="copyParameters">复制参数卡</button>
      <button class="btn-secondary" @tap="goSync">同步给 PC</button>
      <button class="btn-secondary" @tap="goBack">返回分析</button>
    </view>
    <view class="small" style="margin-top: 18rpx;">没有 UMIK-1/REW 实测时，分频/延迟/相位/PEQ 只作起点；深谷不要硬拉。</view>
  </view>
</template>

<script>
import { getCurrentProject, saveProject } from '../../utils/storage.js'
import { createProject, buildRecommendations, applyBalancedLayout } from '../../utils/acoustics.js'

function makeBundle (project) {
  return buildRecommendations(project)
}

export default {
  data () {
    const seed = createProject('载入中')
    return {
      project: seed,
      bundle: makeBundle(seed)
    }
  },
  onShow () {
    this.refresh()
  },
  methods: {
    refresh () {
      this.project = getCurrentProject()
      this.bundle = makeBundle(this.project)
    },
    applyBalanced () {
      try {
        applyBalancedLayout(this.project)
        this.project = saveProject(this.project)
        this.bundle = makeBundle(this.project)
        uni.showToast({ title: '已应用对称摆位', icon: 'none' })
      } catch (error) {
        uni.showModal({ title: '应用失败', content: error.message, showCancel: false })
      }
    },
    saveScheme () {
      try {
        const scheme = this.bundle.schemes.find(item => item.kind === 'free') || this.bundle.schemes[0]
        if (!scheme) return
        this.project.SavedSchemes = this.project.SavedSchemes || []
        this.project.SavedSchemes.push({
          Name: `方案 ${this.project.SavedSchemes.length + 1} · ${scheme.name}`,
          Kind: scheme.kind,
          Summary: scheme.summary,
          ParameterCard: this.bundle.parameters.text,
          CreatedAt: new Date().toISOString()
        })
        this.project = saveProject(this.project)
        uni.showToast({ title: '方案已保存', icon: 'none' })
      } catch (error) {
        uni.showModal({ title: '保存失败', content: error.message, showCancel: false })
      }
    },
    copyParameters () {
      uni.setClipboardData({
        data: this.bundle.parameters.text,
        success: () => uni.showToast({ title: '参数卡已复制', icon: 'none' })
      })
    },
    goSync () {
      uni.switchTab({ url: '/pages/sync/sync' })
    },
    goBack () {
      uni.navigateBack()
    }
  }
}
</script>

<style>
.scheme-head {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  gap: 12rpx;
}
.scheme-name {
  color: #1F2937;
  font-size: 27rpx;
  font-weight: 600;
}
.scheme-score {
  color: #2563EB;
  font-size: 23rpx;
  font-weight: 600;
}
.pros {
  margin-top: 10rpx;
  color: #15803D;
  font-size: 21rpx;
  line-height: 1.55;
}
.cons {
  margin-top: 5rpx;
  color: #B45309;
  font-size: 21rpx;
  line-height: 1.55;
}
.step {
  margin-top: 9rpx;
  color: #334155;
  font-size: 22rpx;
  line-height: 1.55;
}
.parameter-card {
  background-color: #F8FAFC;
}
.parameter-text {
  color: #1F2937;
  font-size: 22rpx;
  line-height: 1.75;
  white-space: pre-wrap;
  font-family: Consolas, "Segoe UI", monospace;
}
</style>
