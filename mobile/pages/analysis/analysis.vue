<template>
  <view class="page">
    <view class="kicker">声学 / 分析</view>
    <view class="title">声学分析</view>
    <view class="subtitle">驻波、Schroeder、RT60、SBIR 与动态余量；没有实测时是估算。</view>

    <view class="summary-grid">
      <view class="summary-card"><text class="summary-label">体积</text><text class="summary-value">{{ format(analysis.volumeM3) }} m³</text></view>
      <view class="summary-card"><text class="summary-label">RT60</text><text class="summary-value">{{ format(analysis.rt60Seconds, 2) }} s</text><text class="summary-extra">{{ format(analysis.rt60Low, 2) }}–{{ format(analysis.rt60High, 2) }} s</text></view>
      <view class="summary-card"><text class="summary-label">Schroeder</text><text class="summary-value">{{ analysis.schroederHz }} Hz</text></view>
      <view class="summary-card"><text class="summary-label">动态余量</text><text class="summary-value">{{ format(analysis.headroomDb) }} dB</text></view>
    </view>

    <view class="card">
      <view class="chart-head">
        <view class="section-title">房间模态分布</view>
        <text class="small">16–300 Hz · {{ analysis.modeCount }} 个</text>
      </view>
      <view class="mode-chart">
        <view v-for="bucket in modeBuckets" :key="bucket.frequency" class="mode-bar-wrap">
          <view class="mode-bar" :style="{ height: bucket.height + '%' }" :class="bucket.type === '轴向' ? 'bar-axial' : bucket.type === '切向' ? 'bar-tangential' : 'bar-oblique'"></view>
        </view>
      </view>
      <view class="axis-row">
        <text>20</text><text>50</text><text>100</text><text>200</text><text>300</text>
      </view>
      <view class="small">柱越高，该频段模态越密；低频座位差异优先靠摆位/多炮/低频陷阱处理。</view>
    </view>

    <view class="card">
      <view class="section-title">风险与处理</view>
      <view v-for="(risk, index) in analysis.risks" :key="index" class="risk-item">
        <view class="risk-head">
          <text class="risk-severity" :class="risk.severity === '高' ? 'sev-high' : risk.severity === '中' ? 'sev-mid' : 'sev-low'">{{ risk.severity }}</text>
          <text class="risk-title">{{ risk.title }}</text>
        </view>
        <view class="muted">{{ risk.detail }}</view>
        <view class="risk-action">{{ risk.action }}</view>
      </view>
    </view>

    <view class="card">
      <view class="section-title">边界干涉 / 第一反射</view>
      <view v-for="(item, index) in analysis.sbir" :key="'s' + index" class="small" style="margin-top: 8rpx;">
        {{ item.speakerRole }} {{ item.surface }}：{{ item.distanceM }} m → {{ item.cancellationHz }} Hz（{{ item.severity }}）
      </view>
      <view v-if="analysis.reflections.length" class="small" style="margin-top: 14rpx;">侧墙第一反射点：{{ analysis.reflections.length }} 个，建议用 5cm 吸音板/厚窗帘处理。</view>
      <view class="small" style="margin-top: 12rpx;">置信度：{{ analysis.confidence }}</view>
    </view>

    <view class="btn-row">
      <button class="btn-secondary" @tap="goRoom">返回修改</button>
      <button class="btn-primary" style="flex: 1;" @tap="goPlan">查看校正方案</button>
    </view>
  </view>
</template>

<script>
import { getCurrentProject } from '../../utils/storage.js'
import { createProject, analyze, getModeBuckets } from '../../utils/acoustics.js'

export default {
  data () {
    const seed = createProject('载入中')
    return {
      project: seed,
      analysis: analyze(seed),
      modeBuckets: getModeBuckets(analyze(seed).modes, 10)
    }
  },
  onShow () {
    this.refresh()
  },
  methods: {
    format (value, digits = 1) {
      const number = Number(value || 0)
      return Number.isFinite(number) ? number.toFixed(digits) : '0'
    },
    refresh () {
      this.project = getCurrentProject()
      this.analysis = analyze(this.project)
      this.modeBuckets = getModeBuckets(this.analysis.modes, 10)
    },
    goRoom () {
      uni.navigateBack()
    },
    goPlan () {
      uni.navigateTo({ url: '/pages/plan/plan' })
    }
  }
}
</script>

<style>
.summary-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14rpx;
  margin-top: 20rpx;
}
.summary-card {
  background-color: #FFFFFF;
  border: 1rpx solid #DCE2EA;
  border-radius: 14rpx;
  padding: 20rpx;
}
.summary-label {
  display: block;
  color: #64748B;
  font-size: 21rpx;
}
.summary-value {
  display: block;
  color: #1F2937;
  font-size: 34rpx;
  font-weight: 700;
  margin-top: 6rpx;
}
.summary-extra {
  display: block;
  color: #94A3B8;
  font-size: 19rpx;
  margin-top: 4rpx;
}
.chart-head {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
}
.mode-chart {
  height: 220rpx;
  display: flex;
  align-items: flex-end;
  gap: 3rpx;
  margin-top: 16rpx;
  border-bottom: 1rpx solid #CBD5E1;
}
.mode-bar-wrap {
  flex: 1;
  height: 100%;
  display: flex;
  align-items: flex-end;
  min-width: 2rpx;
}
.mode-bar {
  width: 100%;
  border-radius: 3rpx 3rpx 0 0;
  background-color: #94A3B8;
}
.bar-axial { background-color: #2563EB; }
.bar-tangential { background-color: #D97706; }
.bar-oblique { background-color: #94A3B8; }
.axis-row {
  display: flex;
  justify-content: space-between;
  color: #94A3B8;
  font-size: 18rpx;
  margin-top: 6rpx;
}
.risk-item {
  margin-top: 16rpx;
  padding-top: 16rpx;
  border-top: 1rpx solid #E2E8F0;
}
.risk-head {
  display: flex;
  align-items: center;
  gap: 10rpx;
}
.risk-severity {
  padding: 3rpx 9rpx;
  border-radius: 6rpx;
  font-size: 19rpx;
}
.sev-high { background-color: #FEF2F2; color: #B91C1C; }
.sev-mid { background-color: #FFF7ED; color: #B45309; }
.sev-low { background-color: #F0FDF4; color: #15803D; }
.risk-title {
  color: #1F2937;
  font-size: 25rpx;
  font-weight: 600;
}
.risk-action {
  margin-top: 8rpx;
  padding: 10rpx 12rpx;
  background-color: #F8FAFC;
  border-radius: 8rpx;
  color: #334155;
  font-size: 21rpx;
  line-height: 1.55;
}
</style>
