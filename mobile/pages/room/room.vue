<template>
  <view class="page">
    <view class="kicker">房间 / 系统</view>
    <view class="title">房间档案</view>
    <view class="subtitle">录入真实尺寸、材质和音箱参数；保存后查看声学分析与方案。</view>

    <view class="card">
      <view class="section-title">房间</view>
      <view class="input-line">
        <text class="label">项目名称</text>
        <input v-model="project.Name" placeholder="项目名称" />
      </view>
      <view class="input-line">
        <text class="label">长/宽/高 (m)</text>
        <input type="digit" v-model="project.Room.LengthM" class="triple" placeholder="长" />
        <input type="digit" v-model="project.Room.WidthM" class="triple" placeholder="宽" />
        <input type="digit" v-model="project.Room.HeightM" class="triple" placeholder="高" />
      </view>
      <view class="input-line">
        <text class="label">形状</text>
        <picker :range="shapeOptions" :value="shapeIndex" @change="onShapeChange">
          <view class="picker-value">{{ project.Room.Shape }}</view>
        </picker>
      </view>
      <view class="input-line">
        <text class="label">地面</text>
        <picker :range="materialNames" :value="floorIndex" @change="onFloorChange">
          <view class="picker-value">{{ project.Room.FloorMaterial }}</view>
        </picker>
      </view>
      <view class="input-line">
        <text class="label">墙面</text>
        <picker :range="materialNames" :value="wallIndex" @change="onWallChange">
          <view class="picker-value">{{ project.Room.WallMaterial }}</view>
        </picker>
      </view>
      <view class="input-line">
        <text class="label">吊顶</text>
        <picker :range="materialNames" :value="ceilingIndex" @change="onCeilingChange">
          <view class="picker-value">{{ project.Room.CeilingMaterial }}</view>
        </picker>
      </view>
      <view class="input-line">
        <text class="label">窗占比 %</text>
        <input type="digit" v-model="project.Room.WindowRatioPercent" />
      </view>
      <view class="input-line">
        <text class="label">家具量</text>
        <picker :range="furnitureOptions" :value="furnitureIndex" @change="onFurnitureChange">
          <view class="picker-value">{{ project.Room.FurnitureLevel }}</view>
        </picker>
      </view>
      <view class="input-line">
        <text class="label">听音位 X / Y (m)</text>
        <input type="digit" v-model="project.Room.ListenerX" class="double" />
        <input type="digit" v-model="project.Room.ListenerY" class="double" />
      </view>
    </view>

    <view class="card">
      <view class="section-title">音箱与动态范围</view>
      <view class="input-line">
        <text class="label">布局</text>
        <picker :range="layoutOptions" :value="layoutIndex" @change="onLayoutChange">
          <view class="picker-value">{{ project.System.Layout }}</view>
        </picker>
      </view>
      <view class="input-line">
        <text class="label">型号</text>
        <input v-model="project.System.SpeakerModel" placeholder="例如：书架箱" />
      </view>
      <view class="input-line">
        <text class="label">低音炮数</text>
        <input type="number" v-model="project.System.SubwooferCount" @blur="onSubwooferChange" />
      </view>
      <view class="input-line">
        <text class="label">低频 / 高频 Hz</text>
        <input type="digit" v-model="project.System.LowFreqHz" class="double" />
        <input type="digit" v-model="project.System.HighFreqHz" class="double" />
      </view>
      <view class="input-line">
        <text class="label">灵敏度/最大声压 (dB)</text>
        <input type="digit" v-model="project.System.SensitivityDb" class="double" />
        <input type="digit" v-model="project.System.MaxSplDb" class="double" />
      </view>
      <view class="input-line">
        <text class="label">动态范围/听音音量 (dB)</text>
        <input type="digit" v-model="project.System.DynamicRangeDb" class="double" />
        <input type="digit" v-model="project.System.ListeningLevelDb" class="double" />
      </view>
      <view class="input-line">
        <text class="label">目标用途</text>
        <picker :range="targetUseOptions" :value="targetUseIndex" @change="onTargetUseChange">
          <view class="picker-value">{{ project.System.TargetUse }}</view>
        </picker>
      </view>
      <view class="input-line">
        <text class="label">目标曲线</text>
        <picker :range="targetCurveOptions" :value="targetCurveIndex" @change="onTargetCurveChange">
          <view class="picker-value">{{ project.System.TargetCurve }}</view>
        </picker>
      </view>
      <view class="small" style="margin-top: 14rpx;">音箱总数：{{ project.System.Speakers.length }}（含低音炮）。没有测量时结果是估算起点。</view>
    </view>

    <view class="card">
      <view class="section-title">平面图</view>
      <canvas id="roomCanvas" canvas-id="roomCanvas" class="plan-canvas"></canvas>
      <view class="small" style="margin-top: 10rpx;">蓝 = 音箱，橙 = 低音炮，青 = 听音位。手机端用于核对相对位置；精确摆放请用 PC 端拖动。</view>
    </view>

    <view class="btn-row">
      <button class="btn-primary" style="flex: 1;" @tap="saveAndAnalyze">保存并查看分析</button>
      <button class="btn-secondary" @tap="resetLayout">重置布局</button>
    </view>
    <view class="status-text">{{ status }}</view>
  </view>
</template>

<script>
import { getCurrentProject, saveProject } from '../../utils/storage.js'
import {
  MATERIAL_NAMES,
  SHAPE_OPTIONS,
  FURNITURE_OPTIONS,
  LAYOUT_OPTIONS,
  TARGET_USE_OPTIONS,
  TARGET_CURVE_OPTIONS,
  seedLayout,
  normalizeProject,
  createProject
} from '../../utils/acoustics.js'

export default {
  data () {
    return {
      project: createProject('载入中'),
      materialNames: MATERIAL_NAMES,
      shapeOptions: SHAPE_OPTIONS,
      furnitureOptions: FURNITURE_OPTIONS,
      layoutOptions: LAYOUT_OPTIONS,
      targetUseOptions: TARGET_USE_OPTIONS,
      targetCurveOptions: TARGET_CURVE_OPTIONS,
      status: '修改后保存，分析会自动刷新。'
    }
  },
  computed: {
    shapeIndex () { return Math.max(0, SHAPE_OPTIONS.indexOf(this.project.Room.Shape)) },
    furnitureIndex () { return Math.max(0, FURNITURE_OPTIONS.indexOf(this.project.Room.FurnitureLevel)) },
    layoutIndex () { return Math.max(0, LAYOUT_OPTIONS.indexOf(this.project.System.Layout)) },
    targetUseIndex () { return Math.max(0, TARGET_USE_OPTIONS.indexOf(this.project.System.TargetUse)) },
    targetCurveIndex () { return Math.max(0, TARGET_CURVE_OPTIONS.indexOf(this.project.System.TargetCurve)) },
    floorIndex () { return Math.max(0, MATERIAL_NAMES.indexOf(this.project.Room.FloorMaterial)) },
    wallIndex () { return Math.max(0, MATERIAL_NAMES.indexOf(this.project.Room.WallMaterial)) },
    ceilingIndex () { return Math.max(0, MATERIAL_NAMES.indexOf(this.project.Room.CeilingMaterial)) }
  },
  onLoad () {
    const current = getCurrentProject()
    this.project = JSON.parse(JSON.stringify(current))
    this.$nextTick(() => this.drawPlan())
  },
  onShow () {
    if (this.project) {
      this.project = JSON.parse(JSON.stringify(getCurrentProject()))
      this.$nextTick(() => this.drawPlan())
    }
  },
  methods: {
    onShapeChange (event) { this.project.Room.Shape = SHAPE_OPTIONS[Number(event.detail.value)] },
    onFurnitureChange (event) { this.project.Room.FurnitureLevel = FURNITURE_OPTIONS[Number(event.detail.value)] },
    onFloorChange (event) { this.project.Room.FloorMaterial = MATERIAL_NAMES[Number(event.detail.value)] },
    onWallChange (event) { this.project.Room.WallMaterial = MATERIAL_NAMES[Number(event.detail.value)] },
    onCeilingChange (event) { this.project.Room.CeilingMaterial = MATERIAL_NAMES[Number(event.detail.value)] },
    onTargetUseChange (event) { this.project.System.TargetUse = TARGET_USE_OPTIONS[Number(event.detail.value)] },
    onTargetCurveChange (event) { this.project.System.TargetCurve = TARGET_CURVE_OPTIONS[Number(event.detail.value)] },
    onLayoutChange (event) {
      this.project.System.Layout = LAYOUT_OPTIONS[Number(event.detail.value)]
      seedLayout(this.project)
      this.$nextTick(() => this.drawPlan())
    },
    onSubwooferChange () {
      const value = Math.max(0, Math.min(4, Number(this.project.System.SubwooferCount) || 0))
      this.project.System.SubwooferCount = value
      seedLayout(this.project)
      this.$nextTick(() => this.drawPlan())
    },
    resetLayout () {
      seedLayout(this.project)
      this.status = '已按当前布局重置音箱位置。'
      this.$nextTick(() => this.drawPlan())
    },
    saveAndAnalyze () {
      try {
        this.project = normalizeProject(this.project)
        saveProject(this.project)
        this.status = '已保存到本机。'
        this.$nextTick(() => this.drawPlan())
        uni.navigateTo({ url: '/pages/analysis/analysis' })
      } catch (error) {
        this.status = `保存失败：${error.message}`
        uni.showModal({ title: '保存失败', content: this.status, showCancel: false })
      }
    },
    drawPlan () {
      if (!this.project) return
      const query = uni.createSelectorQuery().in(this)
      query.select('#roomCanvas').boundingClientRect(rect => {
        if (!rect || !rect.width || !rect.height) return
        const width = rect.width
        const height = rect.height
        const ctx = uni.createCanvasContext('roomCanvas', this)
        const room = this.project.Room
        const scale = Math.min((width - 36) / Math.max(0.1, room.WidthM), (height - 36) / Math.max(0.1, room.LengthM))
        const offsetX = (width - room.WidthM * scale) / 2
        const offsetY = (height - room.LengthM * scale) / 2
        ctx.setFillStyle('#F8FAFC')
        ctx.fillRect(0, 0, width, height)
        ctx.setStrokeStyle('#CBD5E1')
        ctx.setLineWidth(1)
        ctx.strokeRect(offsetX, offsetY, room.WidthM * scale, room.LengthM * scale)
        ctx.setStrokeStyle('#E2E8F0')
        for (let x = 0.5; x < room.WidthM; x += 0.5) {
          ctx.beginPath(); ctx.moveTo(offsetX + x * scale, offsetY); ctx.lineTo(offsetX + x * scale, offsetY + room.LengthM * scale); ctx.stroke()
        }
        for (let y = 0.5; y < room.LengthM; y += 0.5) {
          ctx.beginPath(); ctx.moveTo(offsetX, offsetY + y * scale); ctx.lineTo(offsetX + room.WidthM * scale, offsetY + y * scale); ctx.stroke()
        }
        this.project.System.Speakers.forEach(speaker => {
          const x = offsetX + speaker.X * scale
          const y = offsetY + speaker.Y * scale
          ctx.setFillStyle(speaker.IsSubwoofer ? '#D97706' : '#2563EB')
          ctx.beginPath()
          ctx.arc(x, y, speaker.IsSubwoofer ? 6 : 7, 0, Math.PI * 2)
          ctx.fill()
          ctx.setFillStyle('#334155')
          ctx.setFontSize(9)
          ctx.fillText(speaker.Role, x - 16, y + 18)
        })
        const lx = offsetX + room.ListenerX * scale
        const ly = offsetY + room.ListenerY * scale
        ctx.setFillStyle('#0E7490')
        ctx.beginPath(); ctx.arc(lx, ly, 7, 0, Math.PI * 2); ctx.fill()
        ctx.setFillStyle('#0E7490')
        ctx.setFontSize(9)
        ctx.fillText('听音位', lx - 16, ly + 18)
        ctx.setFillStyle('#64748B')
        ctx.setFontSize(9)
        ctx.fillText(`${room.WidthM.toFixed(1)} × ${room.LengthM.toFixed(1)} m`, 10, 16)
        ctx.draw()
      }).exec()
    }
  }
}
</script>

<style>
.triple {
  width: 30%;
  flex: 1;
}
.double {
  width: 48%;
  flex: 1;
}
.picker-value {
  min-height: 40rpx;
  line-height: 40rpx;
  color: #1F2937;
  font-size: 25rpx;
}
.plan-canvas {
  width: 100%;
  height: 300rpx;
  background-color: #F8FAFC;
  border-radius: 10rpx;
}
.status-text {
  color: #2563EB;
  font-size: 21rpx;
  line-height: 1.6;
  margin-top: 16rpx;
}
</style>
