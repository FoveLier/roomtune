import assert from 'node:assert/strict'
import {
  createProject,
  normalizeProject,
  calculateModes,
  estimateRt60,
  analyze,
  buildRecommendations,
  applyBalancedLayout,
  encodeSync,
  decodeSync,
  getModeBuckets
} from '../utils/acoustics.js'

const project = createProject('手机测试')
assert.equal(project.Name, '手机测试')
assert.ok(project.Room.LengthM > 3)
assert.ok(project.System.Speakers.length >= 3)
assert.ok(estimateRt60(project.Room) > 0)
assert.ok(estimateRt60(project.Room) < 2)

const modes = calculateModes(5.2, 3.8, 2.7, 300)
assert.ok(modes.length > 10)
assert.ok(modes.some(item => Math.abs(item.frequencyHz - 32.98) < 0.9))
assert.ok(modes.every(item => item.frequencyHz >= 16 && item.frequencyHz <= 300))

const analysis = analyze(project)
assert.ok(analysis.volumeM3 > 30)
assert.ok(analysis.rt60Seconds > 0.1)
assert.ok(analysis.schroederHz > 20)
assert.ok(analysis.modes.length === analysis.modeCount)
assert.ok(analysis.risks.length > 0)
assert.ok(analysis.sbir.length > 0)
assert.ok(analysis.reflections.length > 0)
assert.ok(analysis.modes.some(item => item.frequencyHz > 280))

const stereo = createProject('2.0')
stereo.System.Layout = '2.0'
stereo.System.SubwooferCount = 1
const { seedLayout } = await import('../utils/acoustics.js')
seedLayout(stereo)
assert.equal(stereo.System.SubwooferCount, 0)
assert.ok(stereo.System.Speakers.every(item => !item.IsSubwoofer))

const immersive = createProject('7.1.4')
immersive.System.Layout = '7.1.4'
immersive.System.SubwooferCount = 1
seedLayout(immersive)
assert.equal(immersive.System.Speakers.filter(item => item.Role.includes('天空')).length, 4)

const timestamp = project.UpdatedAt
normalizeProject(project)
assert.equal(project.UpdatedAt, timestamp)

let invalidRejected = false
try {
  decodeSync(Buffer.from('{}', 'utf8').toString('base64'))
} catch (error) {
  invalidRejected = true
}
assert.equal(invalidRejected, true)

const bundle = buildRecommendations(project)
assert.equal(bundle.schemes.length, 3)
assert.ok(bundle.treatments.length > 0)
assert.ok(bundle.parameters.peqSuggestions.length > 0)
assert.ok(bundle.parameters.text.includes('分频'))

const copy = normalizeProject(project)
applyBalancedLayout(copy)
assert.equal(copy.Room.ListenerX, copy.Room.WidthM / 2)

const code = encodeSync(project)
assert.ok(code.length > 100)
const decoded = decodeSync(code)
assert.equal(decoded.Name, '手机测试')
assert.ok(Math.abs(decoded.Room.LengthM - project.Room.LengthM) < 0.001)
assert.equal(decoded.System.SpeakerModel, project.System.SpeakerModel)

const buckets = getModeBuckets(analysis.modes)
assert.ok(buckets.length > 0)
assert.ok(buckets.every(item => item.height >= 4))

console.log('mobile core tests passed', JSON.stringify({ modes: modes.length, schemes: bundle.schemes.length, code: code.length }))
