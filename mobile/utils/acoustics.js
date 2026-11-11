/* RoomTune 手机端确定性声学核心。项目字段与 PC 同步码保持一致（PascalCase）。 */
export const SPEED_OF_SOUND = 343
export const TARGET_RT60 = 0.35

export const MATERIALS = [
  { Name: '乳胶漆墙面', Alpha500: 0.03, Alpha1000: 0.04, Description: '硬墙，反射强' },
  { Name: '壁纸墙面', Alpha500: 0.05, Alpha1000: 0.06, Description: '薄壁纸' },
  { Name: '木饰面墙', Alpha500: 0.08, Alpha1000: 0.10, Description: '木饰面/护墙板' },
  { Name: '玻璃窗', Alpha500: 0.03, Alpha1000: 0.02, Description: '大面积玻璃反射' },
  { Name: '厚窗帘', Alpha500: 0.25, Alpha1000: 0.35, Description: '褶皱厚布帘' },
  { Name: '木地板', Alpha500: 0.06, Alpha1000: 0.07, Description: '普通木地板' },
  { Name: '瓷砖/石材', Alpha500: 0.01, Alpha1000: 0.01, Description: '硬质地面' },
  { Name: '厚地毯', Alpha500: 0.30, Alpha1000: 0.45, Description: '厚地毯+垫层' },
  { Name: '石膏板吊顶', Alpha500: 0.05, Alpha1000: 0.06, Description: '普通吊顶' },
  { Name: '吸音板', Alpha500: 0.70, Alpha1000: 0.80, Description: '50–100mm 多孔吸音' },
  { Name: '扩散板', Alpha500: 0.15, Alpha1000: 0.18, Description: '二次余数扩散' },
  { Name: '书架/唱片柜', Alpha500: 0.18, Alpha1000: 0.22, Description: '满架书籍/唱片' }
]

export const MATERIAL_NAMES = MATERIALS.map(item => item.Name)
export const SHAPE_OPTIONS = ['矩形', '长条形', '近方形', 'L形/开放式']
export const FURNITURE_OPTIONS = ['少', '中等', '多']
export const LAYOUT_OPTIONS = ['2.0', '2.1', '5.1', '5.1.2', '7.1.4']
export const TARGET_USE_OPTIONS = ['电影', 'HiFi']
export const TARGET_CURVE_OPTIONS = ['平直', 'Harman', '电影']

function nowText () {
  const date = new Date()
  const p = value => String(value).padStart(2, '0')
  return `${date.getFullYear()}-${p(date.getMonth() + 1)}-${p(date.getDate())} ${p(date.getHours())}:${p(date.getMinutes())}:${p(date.getSeconds())}`
}

function makeId () {
  return `${Date.now().toString(36)}${Math.random().toString(36).slice(2, 8)}`
}

function clone (value) {
  return JSON.parse(JSON.stringify(value))
}

function getMaterial (name) {
  return MATERIALS.find(item => item.Name === name) || MATERIALS[0]
}

function alphaAverage (material) {
  return (Number(material.Alpha500) + Number(material.Alpha1000)) / 2
}

export function createProject (name = '我的客厅影院') {
  const project = {
    Id: makeId(),
    Name: name,
    Notes: '示例房间档案，可直接修改尺寸、材质和音箱参数。',
    CreatedAt: nowText(),
    UpdatedAt: nowText(),
    Room: {
      LengthM: 5.4,
      WidthM: 3.8,
      HeightM: 2.7,
      Shape: '矩形',
      FloorMaterial: '木地板',
      WallMaterial: '乳胶漆墙面',
      CeilingMaterial: '石膏板吊顶',
      WindowRatioPercent: 20,
      FurnitureLevel: '中等',
      ListenerX: 1.9,
      ListenerY: 3.46
    },
    System: {
      Layout: '2.1',
      SpeakerCount: 2,
      SubwooferCount: 1,
      SpeakerModel: '通用书架箱',
      LowFreqHz: 45,
      HighFreqHz: 20000,
      SensitivityDb: 87,
      MaxSplDb: 105,
      DynamicRangeDb: 95,
      ListeningLevelDb: 75,
      TargetUse: '电影',
      TargetCurve: 'Harman',
      Speakers: []
    },
    Measurement: { Source: '', ImportedAt: '', Points: [] },
    SavedSchemes: []
  }
  seedLayout(project)
  return project
}

export function seedLayout (project) {
  const room = project.Room
  const system = project.System
  system.Speakers = []
  if (system.Layout === '2.0') system.SubwooferCount = 0
  const listenerX = Number(room.ListenerX)
  const listenerY = Number(room.ListenerY)
  const frontY = Math.max(0.55, room.LengthM * 0.16)
  const half = Math.max(0.55, Math.min(room.WidthM * 0.30, 1.35))
  const leftX = Math.max(0.45, listenerX - half)
  const rightX = Math.min(room.WidthM - 0.45, listenerX + half)
  const angle = Math.atan2(half, Math.max(0.8, listenerY - frontY)) * 180 / Math.PI
  system.Speakers.push({ Role: '左主箱', X: leftX, Y: frontY, Angle: angle, HeightM: 1.1, IsSubwoofer: false })
  system.Speakers.push({ Role: '右主箱', X: rightX, Y: frontY, Angle: -angle, HeightM: 1.1, IsSubwoofer: false })
  if (system.Layout === '5.1' || system.Layout === '5.1.2' || system.Layout === '7.1.4') {
    system.Speakers.push({ Role: '中置', X: listenerX, Y: Math.max(0.45, frontY - 0.15), Angle: 0, HeightM: 1.1, IsSubwoofer: false })
    system.Speakers.push({ Role: '左环绕', X: Math.max(0.35, room.WidthM * 0.14), Y: listenerY, Angle: 35, HeightM: 1.2, IsSubwoofer: false })
    system.Speakers.push({ Role: '右环绕', X: Math.min(room.WidthM - 0.35, room.WidthM * 0.86), Y: listenerY, Angle: -35, HeightM: 1.2, IsSubwoofer: false })
  }
  if (system.Layout === '5.1.2' || system.Layout === '7.1.4') {
    system.Speakers.push({ Role: '左天空', X: leftX, Y: listenerY - 0.8, Angle: 20, HeightM: 2.2, IsSubwoofer: false })
    system.Speakers.push({ Role: '右天空', X: rightX, Y: listenerY - 0.8, Angle: -20, HeightM: 2.2, IsSubwoofer: false })
  }
  if (system.Layout === '7.1.4') {
    system.Speakers.push({ Role: '左后环绕', X: Math.max(0.3, room.WidthM * 0.12), Y: Math.min(room.LengthM - 0.4, listenerY + 0.9), Angle: 55, HeightM: 1.2, IsSubwoofer: false })
    system.Speakers.push({ Role: '右后环绕', X: Math.min(room.WidthM - 0.3, room.WidthM * 0.88), Y: Math.min(room.LengthM - 0.4, listenerY + 0.9), Angle: -55, HeightM: 1.2, IsSubwoofer: false })
    system.Speakers.push({ Role: '左后天空', X: leftX, Y: Math.min(room.LengthM - 0.4, listenerY + 0.7), Angle: 30, HeightM: 2.2, IsSubwoofer: false })
    system.Speakers.push({ Role: '右后天空', X: rightX, Y: Math.min(room.LengthM - 0.4, listenerY + 0.7), Angle: -30, HeightM: 2.2, IsSubwoofer: false })
  }
  for (let i = 0; i < Math.max(0, Math.min(4, Number(system.SubwooferCount || 0))); i += 1) {
    const subX = system.SubwooferCount === 1 ? listenerX : (i === 0 ? room.WidthM * 0.28 : room.WidthM * 0.72)
    system.Speakers.push({ Role: `低音炮${i + 1}`, X: subX, Y: Math.max(0.25, room.LengthM * 0.10), Angle: 0, HeightM: 0.35, IsSubwoofer: true })
  }
  system.SpeakerCount = system.Speakers.length
}

export function normalizeProject (input) {
  const project = input || createProject()
  project.Id = project.Id || makeId()
  project.Name = project.Name || '未命名房间'
  project.Notes = project.Notes || ''
  project.CreatedAt = project.CreatedAt || nowText()
  project.Room = project.Room || {}
  project.System = project.System || {}
  project.Measurement = project.Measurement || { Source: '', ImportedAt: '', Points: [] }
  project.SavedSchemes = project.SavedSchemes || []
  const room = project.Room
  room.LengthM = clamp(Number(room.LengthM) || 5.4, 2, 20)
  room.WidthM = clamp(Number(room.WidthM) || 3.8, 2, 20)
  room.HeightM = clamp(Number(room.HeightM) || 2.7, 2, 6)
  room.Shape = room.Shape || '矩形'
  room.FloorMaterial = room.FloorMaterial || '木地板'
  room.WallMaterial = room.WallMaterial || '乳胶漆墙面'
  room.CeilingMaterial = room.CeilingMaterial || '石膏板吊顶'
  room.WindowRatioPercent = clamp(Number(room.WindowRatioPercent) || 0, 0, 90)
  room.FurnitureLevel = room.FurnitureLevel || '中等'
  room.ListenerX = clamp(Number(room.ListenerX) || room.WidthM / 2, 0.4, room.WidthM - 0.4)
  room.ListenerY = clamp(Number(room.ListenerY) || room.LengthM * 0.64, 0.4, room.LengthM - 0.4)
  const system = project.System
  system.Layout = system.Layout || '2.1'
  system.LowFreqHz = clamp(Number(system.LowFreqHz) || 45, 20, 500)
  system.HighFreqHz = clamp(Number(system.HighFreqHz) || 20000, 2000, 40000)
  system.SensitivityDb = clamp(Number(system.SensitivityDb) || 87, 70, 110)
  system.MaxSplDb = clamp(Number(system.MaxSplDb) || 105, 85, 140)
  system.DynamicRangeDb = clamp(Number(system.DynamicRangeDb) || 95, 60, 130)
  system.ListeningLevelDb = clamp(Number(system.ListeningLevelDb) || 75, 50, 110)
  system.SubwooferCount = clamp(Number(system.SubwooferCount) || 0, 0, 4)
  system.TargetUse = system.TargetUse || '电影'
  system.TargetCurve = system.TargetCurve || 'Harman'
  if (!Array.isArray(system.Speakers) || system.Speakers.length === 0) seedLayout(project)
  system.Speakers.forEach(item => {
    item.X = clamp(Number(item.X) || 0, 0, room.WidthM)
    item.Y = clamp(Number(item.Y) || 0, 0, room.LengthM)
    item.Angle = clamp(Number(item.Angle) || 0, -90, 90)
  })
  return project
}

function clamp (value, min, max) {
  if (!Number.isFinite(value)) return min
  return Math.max(min, Math.min(max, value))
}

function square (value) {
  return value * value
}

export function estimateAbsorption (room) {
  const floor = getMaterial(room.FloorMaterial)
  const ceiling = getMaterial(room.CeilingMaterial)
  const wall = getMaterial(room.WallMaterial)
  const glass = getMaterial('玻璃窗')
  const floorArea = room.LengthM * room.WidthM
  const wallArea = 2 * (room.LengthM * room.HeightM + room.WidthM * room.HeightM)
  const windowRatio = clamp(room.WindowRatioPercent / 100, 0, 0.9)
  const wallAlpha = alphaAverage(wall) * (1 - windowRatio) + alphaAverage(glass) * windowRatio
  const furniture = room.FurnitureLevel === '少' ? 1.5 : room.FurnitureLevel === '多' ? 7 : 4
  return Math.max(0.5, floorArea * alphaAverage(floor) + floorArea * alphaAverage(ceiling) + wallArea * wallAlpha + furniture)
}

export function estimateRt60 (room) {
  return Math.round(clamp(0.161 * room.LengthM * room.WidthM * room.HeightM / estimateAbsorption(room), 0.15, 1.8) * 100) / 100
}

export function calculateModes (length, width, height, maxHz = 300) {
  const modes = []
  const seen = new Set()
  for (let nx = 0; nx <= 10; nx += 1) {
    for (let ny = 0; ny <= 10; ny += 1) {
      for (let nz = 0; nz <= 10; nz += 1) {
        if (nx === 0 && ny === 0 && nz === 0) continue
        const value = (SPEED_OF_SOUND / 2) * Math.sqrt(square(nx / length) + square(ny / width) + square(nz / height))
        if (value < 16 || value > maxHz) continue
        const type = nx > 0 && ny > 0 && nz > 0 ? '斜向' : ((nx > 0 && ny > 0) || (nx > 0 && nz > 0) || (ny > 0 && nz > 0) ? '切向' : '轴向')
        const key = String(Math.round(value))
        if (seen.has(key)) continue
        seen.add(key)
        modes.push({ frequencyHz: Math.round(value * 10) / 10, type, label: `(${nx},${ny},${nz})`, order: nx + ny + nz, wavelengthM: Math.round(SPEED_OF_SOUND / value * 100) / 100 })
      }
    }
  }
  return modes.sort((a, b) => a.frequencyHz - b.frequencyHz)
}

function addSbir (list, role, surface, distance) {
  if (distance <= 0.05 || distance > 4) return
  const frequency = SPEED_OF_SOUND / (4 * distance)
  if (frequency < 20 || frequency > 300) return
  list.push({ speakerRole: role, surface, distanceM: Math.round(distance * 100) / 100, cancellationHz: Math.round(frequency * 10) / 10, severity: distance < 0.45 ? '高' : distance < 1 ? '中' : '低', note: distance < 0.6 ? '离边界太近，优先挪位或做低频陷阱' : '用吸声/摆位减弱边界干涉' })
}

function computeSbir (project) {
  const list = []
  project.System.Speakers.filter(item => !item.IsSubwoofer).forEach(speaker => {
    addSbir(list, speaker.Role, '前墙', speaker.Y)
    addSbir(list, speaker.Role, '左侧墙', speaker.X)
    addSbir(list, speaker.Role, '右侧墙', project.Room.WidthM - speaker.X)
    addSbir(list, speaker.Role, '地面', speaker.HeightM)
    addSbir(list, speaker.Role, '天花', project.Room.HeightM - speaker.HeightM)
  })
  project.System.Speakers.filter(item => item.IsSubwoofer).forEach(speaker => {
    addSbir(list, speaker.Role, '前墙', speaker.Y)
    addSbir(list, speaker.Role, '左侧墙', speaker.X)
    addSbir(list, speaker.Role, '右侧墙', project.Room.WidthM - speaker.X)
  })
  return list.sort((a, b) => a.cancellationHz - b.cancellationHz)
}

function computeReflections (project) {
  const list = []
  const room = project.Room
  project.System.Speakers.filter(item => !item.IsSubwoofer).forEach(speaker => {
    addReflection(list, speaker, '左侧墙', 0, room.WidthM, room.LengthM, room.ListenerX, room.ListenerY)
    addReflection(list, speaker, '右侧墙', room.WidthM, room.WidthM, room.LengthM, room.ListenerX, room.ListenerY)
  })
  return list
}

function addReflection (list, speaker, surface, wallX, width, length, listenerX, listenerY) {
  const mirrorX = wallX === 0 ? -speaker.X : 2 * width - speaker.X
  const denominator = listenerX - mirrorX
  if (Math.abs(denominator) < 0.0001) return
  const t = (wallX - mirrorX) / denominator
  const py = speaker.Y + t * (listenerY - speaker.Y)
  if (t <= 0 || t >= 1 || py <= 0 || py >= length) return
  const first = Math.sqrt(square(wallX - speaker.X) + square(py - speaker.Y))
  const second = Math.sqrt(square(listenerX - wallX) + square(listenerY - py))
  list.push({ speakerRole: speaker.Role, surface, x: Math.round(wallX * 100) / 100, y: Math.round(py * 100) / 100, distanceM: Math.round((first + second) * 100) / 100 })
}

export function analyze (input) {
  const project = normalizeProject(clone(input))
  const room = project.Room
  const system = project.System
  const volume = room.LengthM * room.WidthM * room.HeightM
  const absorption = estimateAbsorption(room)
  const rt60 = Math.round(clamp(0.161 * volume / Math.max(0.5, absorption), 0.15, 1.8) * 100) / 100
  const modes = calculateModes(room.LengthM, room.WidthM, room.HeightM)
  const sbir = computeSbir(project)
  const reflections = computeReflections(project)
  const headroom = Math.max(0, system.MaxSplDb - system.ListeningLevelDb)
  const analysis = {
    volumeM3: Math.round(volume * 10) / 10,
    rt60Seconds: rt60,
    rt60Low: Math.round(rt60 * 0.82 * 100) / 100,
    rt60High: Math.round(rt60 * 1.18 * 100) / 100,
    schroederHz: Math.round(2000 * Math.sqrt(rt60 / Math.max(1, volume))),
    headroomDb: headroom,
    recommendedMaxBoostDb: Math.round(Math.max(0, Math.min(6, headroom - 6)) * 10) / 10,
    modes,
    modeCount: modes.length,
    sbir,
    reflections,
    confidence: project.Measurement && project.Measurement.Points && project.Measurement.Points.length ? '含 REW 测量参考' : '估算（未导入测量）',
    risks: []
  }
  analysis.risks = buildRisks(project, analysis)
  return analysis
}

function buildRisks (project, analysis) {
  const risks = []
  if (analysis.rt60Seconds > 0.5) {
    risks.push({ severity: '高', title: '混响时间偏长', detail: `估算 RT60 ${analysis.rt60Seconds.toFixed(2)} s，硬墙/硬地面会放大反射。`, action: '先铺厚地毯/挂厚窗帘，再在第一反射点加吸音板；四角优先低频陷阱。' })
  } else if (analysis.rt60Seconds > 0.42) {
    risks.push({ severity: '中', title: '混响略长', detail: 'RT60 处于偏长区间，语言清晰度可能受影响。', action: '用软装增加中高频吸收，保持后墙适度扩散。' })
  } else if (analysis.rt60Seconds < 0.22) {
    risks.push({ severity: '低', title: '房间可能偏干', detail: '估算 RT60 很低，过度吸音会让声音发闷。', action: '减少满墙吸音，改用局部扩散和家具打破平行面。' })
  }
  const room = project.Room
  const maxDim = Math.max(room.LengthM, room.WidthM, room.HeightM)
  const minDim = Math.min(room.LengthM, room.WidthM, room.HeightM)
  if (maxDim / Math.max(0.1, minDim) > 2.3) risks.push({ severity: '中', title: '房间比例偏狭长', detail: '长宽高比例容易让轴向模态分布不均。', action: '听音位避开房间正中，音箱与后墙保留 0.6m 以上。' })
  const low = analysis.modes.filter(item => item.frequencyHz < 150)
  for (let i = 1; i < low.length; i += 1) {
    if (low[i].frequencyHz - low[i - 1].frequencyHz < 2.5) {
      risks.push({ severity: '中', title: '低频模态聚集', detail: `约 ${low[i - 1].frequencyHz.toFixed(1)} Hz 与 ${low[i].frequencyHz.toFixed(1)} Hz 靠得很近。`, action: '尝试 1/4 与 3/4 宽度多炮、改变听音位前后位置；用 REW 实测后再决定 EQ。' })
      break
    }
  }
  if (analysis.sbir.some(item => item.severity === '高')) risks.push({ severity: '高', title: '音箱离边界过近', detail: '存在离墙/离角小于 0.45m 的音箱，SBIR 可能造成低频凹陷。', action: '优先把主箱前移/内收，至少给前墙和侧墙留出 0.6m。' })
  if (analysis.reflections.length) risks.push({ severity: '中', title: '第一反射点需要处理', detail: '侧墙第一反射点会削弱声像与对白清晰度。', action: '在反射点放 5cm 吸音板或厚窗帘；不要整墙铺满。' })
  const left = project.System.Speakers.find(item => item.Role === '左主箱')
  const right = project.System.Speakers.find(item => item.Role === '右主箱')
  if (left && right) {
    const symmetry = Math.abs(room.WidthM - left.X - right.X)
    if (symmetry > 0.35) risks.push({ severity: '中', title: '左右摆位不对称', detail: '左右主箱到侧墙/听音位的距离差异较大，声像可能偏移。', action: '以听音位中心线为轴对称摆放，并让两只主箱 toe-in 角度一致。' })
    if (Math.min(left.Y, right.Y) < 0.6) risks.push({ severity: '中', title: '主箱贴前墙', detail: '主箱离前墙太近，低频边界干涉与低频隆起风险上升。', action: '把主箱前移，至少留 0.6–1.0m。' })
  }
  if (analysis.headroomDb < 8) risks.push({ severity: '高', title: '系统动态余量不足', detail: `最大声压与听音音量只差 ${analysis.headroomDb.toFixed(1)} dB。`, action: '不要用 EQ 硬拉深谷；优先摆位/声装，目标音量留 10 dB 以上余量。' })
  if (room.Shape !== '矩形') risks.push({ severity: '低', title: '非矩形空间为近似计算', detail: '当前使用等效矩形估算。', action: '把本结果作为方向性建议；关键参数用 REW 实测复核。' })
  if (!project.Measurement || !project.Measurement.Points || !project.Measurement.Points.length) risks.push({ severity: '低', title: '当前没有实测曲线', detail: '所有参数卡均为估算起点。', action: '在 PC 端导入 REW CSV，或把实测数据用于复核。' })
  return risks.sort((a, b) => ({ 高: 0, 中: 1, 低: 2 }[a.severity] - { 高: 0, 中: 1, 低: 2 }[b.severity]))
}

function scoreFromAnalysis (analysis) {
  let score = 100
  if (analysis.rt60Seconds > 0.6) score -= 24
  else if (analysis.rt60Seconds > 0.45) score -= 14
  else if (analysis.rt60Seconds < 0.2) score -= 8
  if (analysis.sbir.some(item => item.severity === '高')) score -= 16
  if (analysis.sbir.some(item => item.severity === '中')) score -= 6
  if (analysis.risks.some(item => item.title === '左右摆位不对称')) score -= 10
  if (analysis.risks.some(item => item.title === '低频模态聚集')) score -= 8
  if (analysis.headroomDb < 8) score -= 10
  return Math.max(35, Math.min(98, score))
}

export function applyBalancedLayout (project) {
  project.Room.ListenerX = project.Room.WidthM / 2
  project.Room.ListenerY = Math.max(1.3, Math.min(project.Room.LengthM - 0.8, project.Room.LengthM * 0.64))
  seedLayout(project)
}

function buildTreatments (project, analysis) {
  const items = []
  const volume = analysis.volumeM3
  const target = project.System.TargetUse === 'HiFi' ? 0.40 : 0.32
  const currentAbsorption = estimateAbsorption(project.Room)
  const targetAbsorption = 0.161 * volume / Math.max(0.2, target)
  const missing = Math.max(0, targetAbsorption - currentAbsorption)
  const panelCount = Math.max(2, Math.min(8, Math.ceil(missing / 0.55 / 2)))
  if (analysis.rt60Seconds > target + 0.02) {
    items.push({ priority: 1, title: '四角低频陷阱', location: '四个墙角，从地面到 1.2–1.8m', areaM2: 2, thickness: '30–40cm 厚，留空腔', frequencyRange: '40–200 Hz', budgetLevel: '中', budgetCny: 1800, effect: '压低低频驻波拖尾，改善轰头与座位差异', note: '角落是最有效的低频压力区；租房可用可移动角位陷阱。' })
  }
  if (analysis.rt60Seconds > 0.38 || analysis.reflections.length) {
    items.push({ priority: 2, title: '第一反射点吸音板', location: '左右侧墙镜面反射点，必要时加天花', areaM2: Math.round(panelCount * 0.72 * 10) / 10, thickness: '5cm 多孔吸音，离墙 2–5cm', frequencyRange: '250 Hz–4 kHz', budgetLevel: '中', budgetCny: panelCount * 220, effect: '提升声像稳定度与对白清晰度', note: '先用手持镜法找点；不要整墙铺满，后墙可改扩散。' })
  }
  if (analysis.rt60Seconds > 0.42) {
    items.push({ priority: 2, title: '厚窗帘/厚地毯', location: '窗户与主听音区地面', areaM2: Math.round(project.Room.LengthM * project.Room.WidthM * 0.35 * 10) / 10, thickness: '厚织物/短绒地毯', frequencyRange: '500 Hz–8 kHz', budgetLevel: '低', budgetCny: 600, effect: '降低中高频混响与硬地面反射', note: '软装优先，租房可逆；不要用薄布代替吸音板。' })
  }
  if (analysis.schroederHz > 180) {
    items.push({ priority: 3, title: '后墙扩散/书架', location: '听音位后墙或侧后方', areaM2: 1.2, thickness: '2D 扩散板或满架书架', frequencyRange: '300 Hz–4 kHz', budgetLevel: '中', budgetCny: 800, effect: '打散后墙强反射，避免房间过干', note: '先吸第一反射点，再做扩散；小房间不要过度扩散。' })
  }
  if (!items.length) items.push({ priority: 3, title: '暂不需要大改动', location: '保持现有软装与摆位', areaM2: 0, thickness: '—', frequencyRange: '全频段', budgetLevel: '免费', budgetCny: 0, effect: '当前估算混响已接近目标', note: '用 REW 实测复核 20–300 Hz 再决定。' })
  return items
}

function buildParameterCard (project, analysis) {
  const crossover = Math.max(40, Math.min(120, Math.max(80, project.System.LowFreqHz * 1.5)))
  const distances = []
  const delays = []
  project.System.Speakers.filter(item => !item.IsSubwoofer).forEach(speaker => {
    const distance = Math.sqrt(square(project.Room.ListenerX - speaker.X) + square(project.Room.ListenerY - speaker.Y))
    distances.push(`${speaker.Role} ${distance.toFixed(1)}m`)
    delays.push(`${speaker.Role} ${(distance / SPEED_OF_SOUND * 1000).toFixed(1)}ms`)
  })
  const peq = analysis.modes.filter(item => item.frequencyHz >= 40 && item.frequencyHz <= 200).slice(0, 4).map(item => `${item.frequencyHz.toFixed(1)} Hz：-3.0 dB，Q=8（仅切峰，先实测）`)
  if (!peq.length) peq.push('未发现 40–200 Hz 的显著模态，先不要加 EQ。')
  return {
    crossover: `${Math.round(crossover / 5) * 5} Hz`,
    distances: distances.join('；'),
    delays: delays.join('；'),
    subwoofer: '先设 0° 相位，距离按实测；80 Hz 附近用 REW 比较正/反相，再决定相位与延迟。',
    targetCurve: `${project.System.TargetCurve}（${project.System.TargetUse}）`,
    eqHeadroom: `最大声压 ${project.System.MaxSplDb.toFixed(0)} dB - 听音音量 ${project.System.ListeningLevelDb.toFixed(0)} dB = ${analysis.headroomDb.toFixed(0)} dB；建议最大提升 ${analysis.recommendedMaxBoostDb.toFixed(1)} dB，深谷只做摆位/声装。`,
    peqSuggestions: peq,
    confidence: analysis.confidence,
    text: ''
  }
}

export function buildRecommendations (input) {
  const project = normalizeProject(clone(input))
  const analysis = analyze(project)
  const schemes = []
  const current = { name: '现状微调', kind: 'current', score: scoreFromAnalysis(analysis), summary: '保留现有家具与动线，只做不花钱的摆位/角度微调。', pros: ['零成本、可逆', '不改变现有装修与动线'], cons: ['不能解决房间本身的模态和混响问题'], steps: ['听音位保持在房间长度的 60–70%，避开正中线。', '左右主箱到侧墙距离尽量对称，toe-in 朝听音位。', '低音炮先做前后 30cm 的移动测试。'] }
  schemes.push(current)
  const balancedProject = normalizeProject(clone(project))
  applyBalancedLayout(balancedProject)
  const balancedAnalysis = analyze(balancedProject)
  schemes.push({ name: '对称等边摆位', kind: 'free', score: scoreFromAnalysis(balancedAnalysis), summary: '把听音位移到 64% 长度，主箱与听音位形成近似等边三角形。', pros: ['零成本、通常能改善声像和低频座位差异', '为后续声装/校准提供稳定基线'], cons: ['可能受电视柜、门洞、沙发位置限制', '低音炮最终位置仍需实测'], steps: ['把听音位移到房间长度的约 64%，左右居中。', '左/右主箱前移，离前墙 0.6–1.0m，左右到侧墙尽量对称。', '主箱间距约等于到听音位的距离，toe-in 指向耳朵。', '低音炮尝试前墙 1/4 与 3/4 宽度。'] })
  const treatment = buildTreatments(project, analysis)
  schemes.push({ name: '摆位 + 声装', kind: 'treated', score: Math.min(98, scoreFromAnalysis(balancedAnalysis) + (analysis.rt60Seconds > 0.55 ? 15 : analysis.rt60Seconds > 0.42 ? 10 : 5)), summary: '在对等边摆位基础上处理第一反射点、四角低频陷阱和软装。', pros: ['同时改善中高频清晰度与低频驻波', '比单纯 EQ 更能处理深谷/时域问题'], cons: ['需要预算和施工', '材料参数与施工质量会影响最终效果'], steps: ['先完成“对称等边摆位”，确认听音位与主箱角度。', '左右侧墙第一反射点各放 1–2 块 5cm 吸音板。', '四角放 30–40cm 厚低频陷阱；地面铺厚地毯。', '按预算先软装、后硬装。'] })
  const parameters = buildParameterCard(project, analysis)
  parameters.text = [
    `分频：${parameters.crossover}`,
    `距离：${parameters.distances}`,
    `延迟：${parameters.delays}`,
    `低音炮：${parameters.subwoofer}`,
    `目标曲线：${parameters.targetCurve}`,
    `EQ 余量：${parameters.eqHeadroom}`,
    'PEQ 候选：',
    ...parameters.peqSuggestions.map(item => `  ${item}`),
    `置信度：${parameters.confidence}`
  ].join('\n')
  return { analysis, schemes, treatments: treatment, parameters }
}

export function getModeBuckets (modes, bucketSize = 5) {
  const map = {}
  ;(modes || []).forEach(mode => {
    const key = Math.round(mode.frequencyHz / bucketSize) * bucketSize
    if (!map[key]) map[key] = { count: 0, type: mode.type }
    map[key].count += 1
    if (mode.type === '轴向') map[key].type = '轴向'
    else if (mode.type === '切向' && map[key].type !== '轴向') map[key].type = '切向'
  })
  const max = Math.max(1, ...Object.values(map).map(item => item.count))
  return Object.keys(map).map(Number).sort((a, b) => a - b).map(freq => ({
    frequency: freq,
    count: map[freq].count,
    type: map[freq].type,
    height: Math.max(4, Math.round(map[freq].count / max * 100))
  }))
}

const BASE64 = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/'

function utf8Bytes (text) {
  const bytes = []
  for (let i = 0; i < text.length; i += 1) {
    let code = text.charCodeAt(i)
    if (code < 0x80) bytes.push(code)
    else if (code < 0x800) { bytes.push(0xc0 | (code >> 6), 0x80 | (code & 0x3f)) }
    else if (code < 0xd800 || code >= 0xe000) { bytes.push(0xe0 | (code >> 12), 0x80 | ((code >> 6) & 0x3f), 0x80 | (code & 0x3f)) }
    else { i += 1; code = 0x10000 + (((code & 0x3ff) << 10) | (text.charCodeAt(i) & 0x3ff)); bytes.push(0xf0 | (code >> 18), 0x80 | ((code >> 12) & 0x3f), 0x80 | ((code >> 6) & 0x3f), 0x80 | (code & 0x3f)) }
  }
  return bytes
}

function bytesUtf8 (bytes) {
  let result = ''
  for (let i = 0; i < bytes.length; i += 1) {
    const byte = bytes[i]
    if (byte < 0x80) result += String.fromCharCode(byte)
    else if ((byte & 0xe0) === 0xc0) { result += String.fromCharCode(((byte & 0x1f) << 6) | (bytes[++i] & 0x3f)) }
    else if ((byte & 0xf0) === 0xe0) { result += String.fromCharCode(((byte & 0x0f) << 12) | ((bytes[++i] & 0x3f) << 6) | (bytes[++i] & 0x3f)) }
    else { const code = ((byte & 0x07) << 18) | ((bytes[++i] & 0x3f) << 12) | ((bytes[++i] & 0x3f) << 6) | (bytes[++i] & 0x3f); const offset = code - 0x10000; result += String.fromCharCode(0xd800 + (offset >> 10), 0xdc00 + (offset & 0x3ff)) }
  }
  return result
}

export function encodeSync (project) {
  const bytes = utf8Bytes(JSON.stringify(normalizeProject(clone(project))))
  let output = ''
  for (let i = 0; i < bytes.length; i += 3) {
    const b1 = bytes[i]
    const b2 = bytes[i + 1]
    const b3 = bytes[i + 2]
    const triplet = (b1 << 16) | ((b2 || 0) << 8) | (b3 || 0)
    output += BASE64[(triplet >> 18) & 63]
    output += BASE64[(triplet >> 12) & 63]
    output += i + 1 < bytes.length ? BASE64[(triplet >> 6) & 63] : '='
    output += i + 2 < bytes.length ? BASE64[triplet & 63] : '='
  }
  return output
}

export function decodeSync (code) {
  const clean = String(code || '').replace(/\s/g, '')
  if (!clean) throw new Error('同步码为空')
  const bytes = []
  for (let i = 0; i < clean.length; i += 4) {
    const n1 = BASE64.indexOf(clean[i])
    const n2 = BASE64.indexOf(clean[i + 1])
    const n3 = clean[i + 2] === '=' ? -1 : BASE64.indexOf(clean[i + 2])
    const n4 = clean[i + 3] === '=' ? -1 : BASE64.indexOf(clean[i + 3])
    if (n1 < 0 || n2 < 0) throw new Error('同步码格式无效')
    bytes.push((n1 << 2) | (n2 >> 4))
    if (n3 >= 0) bytes.push(((n2 & 15) << 4) | (n3 >> 2))
    if (n4 >= 0) bytes.push(((n3 & 3) << 6) | n4)
  }
  const parsed = JSON.parse(bytesUtf8(bytes))
  if (!parsed || !parsed.Name || !parsed.Room || !parsed.System) {
    throw new Error('同步码缺少 Name/Room/System，无法导入')
  }
  return normalizeProject(parsed)
}

export function getMaterialNames () {
  return MATERIAL_NAMES.slice()
}
