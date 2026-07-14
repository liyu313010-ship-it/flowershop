<template>
  <canvas ref="canvas" class="particle-field" aria-hidden="true"></canvas>
</template>

<script setup>
import { onBeforeUnmount, onMounted, ref } from 'vue'

const canvas = ref(null)
let context = null
let animationFrame = 0
let resizeObserver = null
let particles = []
let width = 0
let height = 0
let dpr = 1
let reducedMotion = false
let lowPower = false
const pointer = { x: -1000, y: -1000, active: false }

const createParticles = () => {
  const count = reducedMotion ? 0 : Math.min(lowPower ? 36 : 72, Math.max(lowPower ? 14 : 28, Math.round(width / 22)))
  particles = Array.from({ length: count }, (_, index) => ({
    x: Math.random() * width,
    y: Math.random() * height,
    vx: (Math.random() - 0.5) * 0.22,
    vy: (Math.random() - 0.5) * 0.22,
    radius: index % 7 === 0 ? 2.2 : 1 + Math.random() * 1.25,
    alpha: 0.22 + Math.random() * 0.42,
    tone: index % 4 === 0 ? 'pink' : 'white'
  }))
}

const resize = () => {
  if (!canvas.value) return
  const rect = canvas.value.getBoundingClientRect()
  width = rect.width
  height = rect.height
  dpr = Math.min(window.devicePixelRatio || 1, 2)
  canvas.value.width = Math.round(width * dpr)
  canvas.value.height = Math.round(height * dpr)
  context = canvas.value.getContext('2d')
  context?.setTransform(dpr, 0, 0, dpr, 0, 0)
  createParticles()
}

const updatePointer = (event) => {
  if (!canvas.value) return
  const rect = canvas.value.getBoundingClientRect()
  pointer.x = event.clientX - rect.left
  pointer.y = event.clientY - rect.top
  pointer.active = pointer.x >= 0 && pointer.x <= width && pointer.y >= 0 && pointer.y <= height
}

const draw = () => {
  if (!context || !width || !height) return
  context.clearRect(0, 0, width, height)
  const linkDistance = 118

  particles.forEach((particle) => {
    if (pointer.active) {
      const dx = pointer.x - particle.x
      const dy = pointer.y - particle.y
      const distance = Math.hypot(dx, dy)
      if (distance < 150 && distance > 1) {
        const force = (150 - distance) / 150
        particle.vx += (dx / distance) * force * 0.006
        particle.vy += (dy / distance) * force * 0.006
      }
    }
    particle.vx *= 0.995
    particle.vy *= 0.995
    particle.x += particle.vx
    particle.y += particle.vy
    if (particle.x < -10) particle.x = width + 10
    if (particle.x > width + 10) particle.x = -10
    if (particle.y < -10) particle.y = height + 10
    if (particle.y > height + 10) particle.y = -10
  })

  for (let i = 0; i < particles.length; i += 1) {
    const particle = particles[i]
    for (let j = i + 1; j < particles.length; j += 1) {
      const other = particles[j]
      const distance = Math.hypot(particle.x - other.x, particle.y - other.y)
      if (distance < linkDistance) {
        context.beginPath()
        context.moveTo(particle.x, particle.y)
        context.lineTo(other.x, other.y)
        context.strokeStyle = `rgba(255, 255, 255, ${(1 - distance / linkDistance) * 0.12})`
        context.lineWidth = 0.7
        context.stroke()
      }
    }
    if (pointer.active) {
      const distance = Math.hypot(pointer.x - particle.x, pointer.y - particle.y)
      if (distance < 142) {
        context.beginPath()
        context.moveTo(pointer.x, pointer.y)
        context.lineTo(particle.x, particle.y)
        context.strokeStyle = `rgba(255, 213, 226, ${(1 - distance / 142) * 0.34})`
        context.lineWidth = 1
        context.stroke()
      }
    }
    context.beginPath()
    context.arc(particle.x, particle.y, particle.radius, 0, Math.PI * 2)
    context.fillStyle = particle.tone === 'pink'
      ? `rgba(255, 192, 214, ${particle.alpha})`
      : `rgba(255, 255, 255, ${particle.alpha})`
    context.shadowColor = 'rgba(255, 173, 202, .45)'
    context.shadowBlur = particle.radius > 2 ? 8 : 3
    context.fill()
    context.shadowBlur = 0
  }
  animationFrame = window.requestAnimationFrame(draw)
}

onMounted(() => {
  const motionQuery = window.matchMedia('(prefers-reduced-motion: reduce)')
  reducedMotion = motionQuery.matches
  // 低性能设备降低粒子密度，避免影响商品列表滚动和结算交互。
  lowPower = Boolean(navigator.hardwareConcurrency && navigator.hardwareConcurrency <= 4)
  if (lowPower && canvas.value) canvas.value.style.opacity = '.55'
  resize()
  if (typeof ResizeObserver !== 'undefined') {
    resizeObserver = new ResizeObserver(resize)
    if (canvas.value) resizeObserver.observe(canvas.value)
  }
  window.addEventListener('pointermove', updatePointer, { passive: true })
  if (!reducedMotion) animationFrame = window.requestAnimationFrame(draw)
})

onBeforeUnmount(() => {
  window.cancelAnimationFrame(animationFrame)
  window.removeEventListener('pointermove', updatePointer)
  resizeObserver?.disconnect()
})
</script>

<style scoped>
.particle-field { display: block; width: 100%; height: 100%; pointer-events: none; mix-blend-mode: screen; opacity: .82; }
</style>
