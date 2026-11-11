import fs from 'node:fs'

const [url, out, setupExpr = '', clickExpr = '', waitMs = '200'] = process.argv.slice(2)
const port = process.env.CDP_PORT || '9222'
const list = await fetch(`http://127.0.0.1:${port}/json/list`).then(r => r.json())
const target = list.find(t => t.type === 'page' && t.url.includes('localhost:5173')) ||
	list.find(t => t.type === 'page')
if (!target) throw new Error('no Chrome page target on 9222')

const ws = new WebSocket(target.webSocketDebuggerUrl)
let nextId = 1
const pending = new Map()

ws.addEventListener('message', event => {
	const msg = JSON.parse(event.data)
	if (msg.id && pending.has(msg.id)) {
		const item = pending.get(msg.id)
		pending.delete(msg.id)
		if (msg.error) item.reject(new Error(JSON.stringify(msg.error)))
		else item.resolve(msg.result)
	}
})

await new Promise((resolve, reject) => {
	ws.addEventListener('open', resolve, { once: true })
	ws.addEventListener('error', reject, { once: true })
})

function send(method, params = {}) {
	const id = nextId++
	return new Promise((resolve, reject) => {
		pending.set(id, { resolve, reject })
		ws.send(JSON.stringify({ id, method, params }))
	})
}

const sleep = ms => new Promise(resolve => setTimeout(resolve, ms))

await send('Page.enable')
await send('Runtime.enable')
await send('Emulation.setDeviceMetricsOverride', {
	width: 390,
	height: 844,
	deviceScaleFactor: 1,
	mobile: true
})
await send('Page.navigate', { url })
await sleep(2500)

if (setupExpr) {
	await send('Runtime.evaluate', {
		expression: setupExpr,
		awaitPromise: true,
		returnByValue: true
	})
}

if (clickExpr) {
	await send('Runtime.evaluate', {
		expression: clickExpr,
		awaitPromise: true,
		returnByValue: true
	})
}

await sleep(Number(waitMs))
const shot = await send('Page.captureScreenshot', {
	format: 'png',
	captureBeyondViewport: false
})
fs.writeFileSync(out, Buffer.from(shot.data, 'base64'))
console.log(out)
ws.close()
