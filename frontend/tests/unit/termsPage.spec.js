import { readFileSync } from 'node:fs'
import { resolve } from 'node:path'

const readSource = path => readFileSync(resolve(process.cwd(), path), 'utf8')

describe('服务条款页面', () => {
  const termsSource = readSource('src/views/Terms.vue')
  const routerSource = readSource('src/router/index.js')
  const footerSource = readSource('src/components/layout/Footer.vue')

  test('注册公开的 /terms 路由且不会再进入 404', () => {
    expect(routerSource).toContain("path: '/terms'")
    expect(routerSource).toContain("name: 'Terms'")
    expect(routerSource.indexOf("path: '/terms'")).toBeLessThan(
      routerSource.indexOf("path: '/:pathMatch(.*)*'")
    )
  })

  test('包含交易所需的核心条款章节', () => {
    ;['账户注册与安全', '商品信息与订单成立', '价格、优惠与支付', '配送与签收',
      '售后、退换与退款', '用户行为与内容规范', '隐私保护与知识产权',
      '服务变更与责任边界'].forEach(title => {
      expect(termsSource).toContain(title)
    })
  })

  test('页脚提供清晰的服务条款入口', () => {
    expect(footerSource).toContain('href="/terms"')
    expect(footerSource).toContain('服务条款')
  })
})
