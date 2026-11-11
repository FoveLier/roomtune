import { createProject, normalizeProject } from './acoustics.js'

const PROJECTS_KEY = 'roomtune_projects_v1'
const CURRENT_KEY = 'roomtune_current_v1'

function timestamp () {
  const date = new Date()
  const p = value => String(value).padStart(2, '0')
  return `${date.getFullYear()}-${p(date.getMonth() + 1)}-${p(date.getDate())} ${p(date.getHours())}:${p(date.getMinutes())}:${p(date.getSeconds())}`
}

function read (key, fallback) {
  try {
    const value = uni.getStorageSync(key)
    if (value === '' || value === null || typeof value === 'undefined') return fallback
    return typeof value === 'string' ? JSON.parse(value) : value
  } catch (error) {
    return fallback
  }
}

function write (key, value) {
  try {
    uni.setStorageSync(key, value)
    return true
  } catch (error) {
    console.warn('[storage] write failed', key)
    return false
  }
}

function backupCorrupt (value) {
  try {
    uni.setStorageSync(`${PROJECTS_KEY}_corrupt_${Date.now()}`, value)
  } catch (error) {
    // Keep the original key untouched even if the backup cannot be written.
  }
}

let lastStorageError = ''

export function getStorageError () {
  return lastStorageError
}

function readProjectList () {
  lastStorageError = ''
  let stored
  try {
    stored = uni.getStorageSync(PROJECTS_KEY)
  } catch (error) {
    lastStorageError = '无法读取本机 Storage'
    return { projects: [], error: lastStorageError }
  }
  if (stored === '' || stored === null || typeof stored === 'undefined') return { projects: [], error: '' }
  let value = stored
  if (typeof value === 'string') {
    try {
      value = JSON.parse(value)
    } catch (error) {
      backupCorrupt(stored)
      lastStorageError = '项目 Storage 不是有效 JSON，已备份原值，未覆盖'
      return { projects: [], error: lastStorageError }
    }
  }
  if (!Array.isArray(value)) {
    backupCorrupt(stored)
    lastStorageError = '项目 Storage 结构异常，已备份原值，未覆盖'
    return { projects: [], error: lastStorageError }
  }
  return { projects: value.map(item => normalizeProject(item)), error: '' }
}

export function loadProjects () {
  const result = readProjectList()
  if (result.error) return []
  if (!result.projects.length) return []
  // Refresh normalized copies without changing UpdatedAt.
  write(PROJECTS_KEY, result.projects)
  return result.projects
}

export function saveProjects (projects) {
  const normalized = (projects || []).map(item => normalizeProject(item))
  if (!write(PROJECTS_KEY, normalized)) throw new Error('写入本机 Storage 失败')
  return normalized
}

export function getCurrentProject () {
  const projects = loadProjects()
  if (!projects.length) return null
  const currentId = read(CURRENT_KEY, '')
  const project = projects.find(item => item.Id === currentId) || projects[0]
  if (project) write(CURRENT_KEY, project.Id)
  return project
}

export function setCurrentProject (id) {
  if (!id) throw new Error('项目 ID 为空')
  if (!write(CURRENT_KEY, id)) throw new Error('切换当前项目失败')
  return getProject(id)
}

export function getProject (id) {
  return loadProjects().find(item => item.Id === id) || null
}

export function saveProject (project) {
  const normalized = normalizeProject(project)
  normalized.UpdatedAt = timestamp()
  const projects = loadProjects()
  const index = projects.findIndex(item => item.Id === normalized.Id)
  if (index >= 0) projects[index] = normalized
  else projects.unshift(normalized)
  if (!write(PROJECTS_KEY, projects) || !write(CURRENT_KEY, normalized.Id)) {
    throw new Error('保存失败：无法写入本机 Storage')
  }
  return normalized
}

export function createNewProject (name) {
  const project = createProject(name || '新房间')
  project.UpdatedAt = timestamp()
  const projects = loadProjects()
  projects.unshift(project)
  if (!write(PROJECTS_KEY, projects) || !write(CURRENT_KEY, project.Id)) {
    throw new Error('新建失败：无法写入本机 Storage')
  }
  return project
}

export function deleteProject (id) {
  const projects = loadProjects().filter(item => item.Id !== id)
  if (!projects.length) {
    if (!write(PROJECTS_KEY, []) || !write(CURRENT_KEY, '')) {
      throw new Error('删除失败：无法写入本机 Storage')
    }
    return []
  }
  if (read(CURRENT_KEY, '') === id) write(CURRENT_KEY, projects[0].Id)
  if (!write(PROJECTS_KEY, projects)) throw new Error('删除失败：无法写入本机 Storage')
  return projects
}

export function importProject (project) {
  const copy = normalizeProject(JSON.parse(JSON.stringify(project)))
  copy.Id = `${Date.now().toString(36)}${Math.random().toString(36).slice(2, 7)}`
  copy.Name = `${copy.Name}（同步）`
  copy.UpdatedAt = timestamp()
  return saveProject(copy)
}

export function clearAllProjects () {
  if (!write(PROJECTS_KEY, []) || !write(CURRENT_KEY, '')) {
    throw new Error('清空失败：无法写入本机 Storage')
  }
  return null
}

export function getStorageInfo () {
  try {
    const info = uni.getStorageInfoSync()
    return { keys: info.keys || [], currentSize: info.currentSize || 0, limitSize: info.limitSize || 0 }
  } catch (error) {
    return { keys: [], currentSize: 0, limitSize: 0 }
  }
}
