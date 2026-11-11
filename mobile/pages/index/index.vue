<template>
  <view class="page">
    <view class="kicker">手机工作台</view>
    <view class="title">声居</view>
    <view class="subtitle">把房间和音箱参数，变成可执行的摆位、声装与校准建议。</view>
    <view v-if="storageError" class="card storage-error">{{ storageError }}</view>

    <view class="card">
      <view class="section-title">新建房间</view>
      <view class="create-row">
        <input v-model="newName" class="name-input" placeholder="例如：客厅影院" />
        <button class="btn-primary create-btn" @tap="createProject">新建</button>
      </view>
      <view class="small" style="margin-top: 10rpx;">数据保存在本机 App 内；与 PC 通过同步码配合。</view>
    </view>

    <view class="section-title" style="margin-top: 28rpx;">我的项目</view>
    <view v-for="project in projects" :key="project.Id" class="card project-card" @tap="openProject(project)">
      <view class="project-main">
        <view class="project-name">{{ project.Name }}</view>
        <view class="muted">{{ project.Room.LengthM }} × {{ project.Room.WidthM }} × {{ project.Room.HeightM }} m · {{ project.System.Layout }}</view>
        <view class="small">更新：{{ project.UpdatedAt }}</view>
      </view>
      <view class="project-actions">
        <text class="badge">打开</text>
        <text class="delete" @tap.stop="deleteProject(project)">删除</text>
      </view>
    </view>

    <view v-if="!projects.length" class="card muted">还没有项目，先新建一个房间档案。</view>
    <view class="small" style="margin-top: 26rpx; text-align: center;">RoomTune · 手机端独立可用</view>
  </view>
</template>

<script>
import { loadProjects, createNewProject, deleteProject, setCurrentProject, getStorageError } from '../../utils/storage.js'

export default {
  data () {
    return {
      projects: [],
      newName: '新房间',
      storageError: ''
    }
  },
  onShow () {
    this.refresh()
  },
  methods: {
    refresh () {
      this.projects = loadProjects()
      this.storageError = getStorageError()
    },
    createProject () {
      try {
        const name = (this.newName || '').trim() || '新房间'
        const project = createNewProject(name)
        this.newName = '新房间'
        this.refresh()
        this.openProject(project)
      } catch (error) {
        uni.showModal({ title: '新建失败', content: error.message, showCancel: false })
      }
    },
    openProject (project) {
      try {
        setCurrentProject(project.Id)
        uni.navigateTo({ url: '/pages/room/room' })
      } catch (error) {
        uni.showModal({ title: '打开失败', content: error.message, showCancel: false })
      }
    },
    deleteProject (project) {
      uni.showModal({
        title: '删除项目',
        content: `删除“${project.Name}”？此操作不可撤销。`,
        success: result => {
          if (!result.confirm) return
          try {
            deleteProject(project.Id)
            this.refresh()
          } catch (error) {
            uni.showModal({ title: '删除失败', content: error.message, showCancel: false })
          }
        }
      })
    }
  }
}
</script>

<style>
.create-row {
  display: flex;
  gap: 14rpx;
  margin-top: 16rpx;
}
.storage-error {
  color: #B45309;
  background-color: #FFF7ED;
  border-color: #FED7AA;
  font-size: 22rpx;
  line-height: 1.6;
}
.name-input {
  flex: 1;
  height: 76rpx;
  border: 1rpx solid #DCE2EA;
  border-radius: 10rpx;
  padding: 0 18rpx;
  font-size: 25rpx;
  background-color: #F8FAFC;
}
.create-btn {
  width: 150rpx;
  flex: 0 0 150rpx;
}
.project-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 14rpx;
}
.project-main {
  flex: 1;
  min-width: 0;
}
.project-name {
  color: #1F2937;
  font-size: 30rpx;
  font-weight: 600;
  line-height: 1.4;
}
.project-actions {
  flex: 0 0 auto;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 12rpx;
}
.delete {
  color: #B91C1C;
  font-size: 21rpx;
  padding: 4rpx;
}
</style>
