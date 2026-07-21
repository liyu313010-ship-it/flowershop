<template>
  <main class="terms-page">
    <div class="ambient ambient-one" aria-hidden="true"></div>
    <div class="ambient ambient-two" aria-hidden="true"></div>

    <section class="terms-hero">
      <div class="hero-mark" aria-hidden="true">
        <svg viewBox="0 0 64 64" fill="none">
          <path d="M32 51V27" stroke="currentColor" stroke-width="3" stroke-linecap="round" />
          <path d="M32 36C24 30 17 32 14 38c8 3 14 2 18-2Z" fill="currentColor" opacity=".48" />
          <path d="M32 31c8-6 15-4 18 2-8 3-14 2-18-2Z" fill="currentColor" opacity=".72" />
          <path d="M32 27c-7-2-10-8-7-15 7 2 10 8 7 15Z" fill="#f7a4bd" />
          <path d="M33 27c7-2 10-8 7-15-7 2-10 8-7 15Z" fill="#df5f91" />
          <circle cx="32" cy="27" r="4" fill="#f3bd6b" />
        </svg>
      </div>
      <p class="eyebrow">HUANYU FLOWER · SERVICE AGREEMENT</p>
      <h1>服务条款</h1>
      <p class="hero-copy">
        这份条款说明您使用欢雨鲜花网站、选购商品及享受配送与售后服务时，双方需要共同遵守的规则。
      </p>
      <div class="hero-meta" aria-label="条款版本信息">
        <span>版本号：2026.07</span>
        <span>发布日期：2026年7月21日</span>
        <span>生效日期：2026年7月21日</span>
      </div>
    </section>

    <section class="promise-grid" aria-label="服务原则">
      <article class="promise-card">
        <span class="promise-icon">
          <svg viewBox="0 0 24 24" fill="none"><path d="m7 12 3 3 7-7" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/><circle cx="12" cy="12" r="9" stroke="currentColor" stroke-width="1.5"/></svg>
        </span>
        <div><strong>规则透明</strong><p>价格、配送与售后规则清晰展示</p></div>
      </article>
      <article class="promise-card">
        <span class="promise-icon">
          <svg viewBox="0 0 24 24" fill="none"><path d="M7 10V8a5 5 0 0 1 10 0v2M6 10h12v10H6V10Z" stroke="currentColor" stroke-width="1.6" stroke-linejoin="round"/></svg>
        </span>
        <div><strong>交易安心</strong><p>订单信息与账户数据受到保护</p></div>
      </article>
      <article class="promise-card">
        <span class="promise-icon">
          <svg viewBox="0 0 24 24" fill="none"><path d="M5 13a7 7 0 0 1 14 0v5h-3v-5h3M5 13v5h3v-5H5Zm3 6c1 1 2.3 1.5 4 1.5" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"/></svg>
        </span>
        <div><strong>售后有回应</strong><p>遇到问题可随时联系在线客服</p></div>
      </article>
    </section>

    <div class="terms-layout">
      <aside class="terms-nav" aria-label="服务条款目录">
        <p class="nav-label">条款目录</p>
        <a v-for="item in sections" :key="item.id" :href="`#${item.id}`">
          <span>{{ item.number }}</span>{{ item.title }}
        </a>
        <div class="nav-help">
          <span>对条款有疑问？</span>
          <RouterLink to="/contact">联系我们</RouterLink>
        </div>
      </aside>

      <article class="terms-content">
        <div class="notice-card">
          <span class="notice-icon" aria-hidden="true">!</span>
          <div>
            <strong>请特别留意</strong>
            <p>鲜花属于生鲜易腐商品，商品的颜色、开放度和枝形会自然存在差异，退换与退款条件也不同于普通耐用品。下单前请重点阅读商品说明、配送规则和退换货政策。</p>
          </div>
        </div>

        <section id="scope" class="term-section">
          <div class="section-heading"><span>01</span><div><p>Scope</p><h2>条款的确认与适用范围</h2></div></div>
          <p>欢迎使用欢雨鲜花网站及相关服务。本条款适用于您浏览网站、注册账户、购买商品、提交评价、使用在线客服及其他由欢雨鲜花提供的功能。</p>
          <p>当您注册、下单或继续使用本网站时，即表示您已经阅读并同意本条款及与具体服务相关的页面规则。如您不同意其中任何内容，请停止使用相关服务并联系我们。</p>
        </section>

        <section id="account" class="term-section">
          <div class="section-heading"><span>02</span><div><p>Account</p><h2>账户注册与安全</h2></div></div>
          <ul>
            <li>您应提供真实、准确、完整的注册和收货信息，并在信息发生变化时及时更新。</li>
            <li>账户仅限本人使用。请妥善保管登录凭证，不得出租、转让或以其他方式允许他人违规使用。</li>
            <li>如发现账户被异常登录、冒用或存在其他安全风险，请立即修改密码并联系客服。</li>
            <li>因您主动泄露凭证或未妥善保管设备造成的损失，我们会协助处理，但相应责任依法由责任方承担。</li>
          </ul>
        </section>

        <section id="products" class="term-section">
          <div class="section-heading"><span>03</span><div><p>Products & Orders</p><h2>商品信息与订单成立</h2></div></div>
          <p>我们会尽力确保商品名称、图片、花材、规格、价格和库存信息准确。因鲜花具有季节性和自然差异，实际商品可能在花朵大小、色泽、开放度、枝形和包装细节上与展示图片略有不同。</p>
          <ul>
            <li>订单提交成功不代表交易必然完成；订单经系统确认且库存可满足后正式进入履约流程。</li>
            <li>如遇花材缺货，我们会优先联系您，并在取得同意后使用价值和风格相近的花材替换。</li>
            <li>如出现明显标价错误、库存异常或不可抗力导致无法履约，我们会及时说明并提供取消、退款或协商替代方案。</li>
            <li>定制花礼一旦进入制作阶段，除质量问题外，可能无法取消或修改。</li>
          </ul>
        </section>

        <section id="payment" class="term-section">
          <div class="section-heading"><span>04</span><div><p>Price & Payment</p><h2>价格、优惠与支付</h2></div></div>
          <p>商品价格、配送费、优惠金额和应付总额以结算页面最终显示为准。可用支付方式以您下单时页面实际提供的选项为准。</p>
          <ul>
            <li>优惠券、折扣和活动不得折现，使用范围、有效期及叠加规则以活动页面说明为准。</li>
            <li>支付完成后请保留订单编号；如付款成功但订单状态未更新，请勿重复支付并及时联系客服核查。</li>
            <li>因退款产生的到账时间受支付机构处理周期影响，具体时间以银行或支付平台规则为准。</li>
          </ul>
        </section>

        <section id="delivery" class="term-section">
          <div class="section-heading"><span>05</span><div><p>Delivery</p><h2>配送与签收</h2></div></div>
          <p>您应提供准确的收货人姓名、联系方式、配送地址和期望配送时间，并确保收货电话畅通。配送范围、费用和预计时效以<RouterLink to="/shipping">《配送说明》</RouterLink>及结算页展示为准。</p>
          <ul>
            <li>因地址或电话错误、无人接听、收货人拒收等用户原因产生的再次配送费用，可能需要由用户承担。</li>
            <li>节日订单高峰、交通管制、极端天气等情况可能造成延迟，我们会尽力协调并及时沟通。</li>
            <li>请在签收后尽快检查商品。如发现严重破损、错漏或明显质量问题，请保留商品与包装并及时拍照联系客服。</li>
          </ul>
        </section>

        <section id="after-sales" class="term-section">
          <div class="section-heading"><span>06</span><div><p>After-sales</p><h2>售后、退换与退款</h2></div></div>
          <p>鲜花属于生鲜易腐商品，通常不适用无理由退货。若商品存在严重质量问题、配送破损、品类或数量与订单明显不符，请在签收后尽快通过在线客服提交订单号、问题描述及清晰照片。</p>
          <p>我们将依据问题情况提供补送、更换、部分退款或全额退款等方案。具体条件、申请流程和处理时效请参阅<RouterLink to="/returns">《退换货政策》</RouterLink>。</p>
        </section>

        <section id="conduct" class="term-section">
          <div class="section-heading"><span>07</span><div><p>User Conduct</p><h2>用户行为与内容规范</h2></div></div>
          <p>您可以依法发表真实的商品评价并使用客服功能，但不得发布违法、侮辱、虚假、侵权、广告引流、恶意刷单或危害系统安全的内容，不得利用网站实施欺诈、干扰交易秩序或侵害他人权益。</p>
          <p>对违反规则的内容或行为，我们可在法律允许范围内采取提示、删除内容、限制功能、暂停账户等必要措施；涉嫌违法犯罪的，将依法配合有关机关处理。</p>
        </section>

        <section id="rights" class="term-section">
          <div class="section-heading"><span>08</span><div><p>Privacy & Rights</p><h2>隐私保护与知识产权</h2></div></div>
          <p>我们依照<RouterLink to="/privacy">《隐私政策》</RouterLink>处理您的账户、订单、收货和客服信息，仅在完成交易、提供服务、保障安全或履行法定义务所需范围内使用。</p>
          <p>网站中的品牌标识、页面设计、文字、图片和程序等内容受法律保护。未经权利人书面许可，不得擅自复制、抓取、改编、传播或用于商业用途。商品评价中由您原创的内容，其合法权利仍归您所有。</p>
        </section>

        <section id="liability" class="term-section">
          <div class="section-heading"><span>09</span><div><p>Liability</p><h2>服务变更与责任边界</h2></div></div>
          <p>我们会持续维护网站，但不保证服务在任何时间都不间断。因系统维护、网络故障、第三方服务异常、自然灾害、公共卫生事件、政府行为等合理控制范围之外的原因造成服务延迟或中断时，我们会尽力恢复并减少影响。</p>
          <p>我们不会通过本条款排除或限制依法不得排除或限制的消费者权利及责任。其他损失将依据实际过错、因果关系和适用法律合理确定。</p>
        </section>

        <section id="contact" class="term-section contact-section">
          <div class="section-heading"><span>10</span><div><p>Updates & Contact</p><h2>条款更新、争议解决与联系</h2></div></div>
          <p>为适应业务和法律要求，我们可能更新本条款。重大变化会通过网站提示等合理方式告知；更新后的版本将在标明的日期生效。更新前已经成立的订单，原则上仍按下单时适用的规则处理。</p>
          <p>如发生争议，双方应先友好协商；协商不成的，可依法向有管辖权的人民法院或有关消费者权益保护机构寻求解决。</p>
          <div class="contact-panel">
            <div><span>客服电话</span><a href="tel:15025033613">15025033613</a></div>
            <div><span>电子邮箱</span><a href="mailto:2784107771@qq.com">2784107771@qq.com</a></div>
            <div><span>服务时间</span><strong>每日 8:00–20:00</strong></div>
          </div>
        </section>

        <footer class="terms-end">
          <div><span aria-hidden="true">✦</span></div>
          <h2>谢谢您认真阅读</h2>
          <p>愿每一次选花与收花，都清楚、安心，也充满期待。</p>
          <div class="end-actions">
            <RouterLink to="/products" class="primary-action">去挑一束花</RouterLink>
            <RouterLink to="/contact" class="secondary-action">咨询客服</RouterLink>
          </div>
        </footer>
      </article>
    </div>
  </main>
</template>

<script setup>
const sections = [
  { id: 'scope', number: '01', title: '确认与适用' },
  { id: 'account', number: '02', title: '账户安全' },
  { id: 'products', number: '03', title: '商品与订单' },
  { id: 'payment', number: '04', title: '价格与支付' },
  { id: 'delivery', number: '05', title: '配送与签收' },
  { id: 'after-sales', number: '06', title: '售后与退款' },
  { id: 'conduct', number: '07', title: '用户行为' },
  { id: 'rights', number: '08', title: '隐私与权利' },
  { id: 'liability', number: '09', title: '责任边界' },
  { id: 'contact', number: '10', title: '更新与联系' }
]
</script>

<style scoped>
.terms-page {
  --rose: #c64f7e;
  --rose-dark: #7a2f50;
  --ink: #402535;
  --muted: #866b79;
  position: relative;
  overflow: hidden;
  min-height: 100vh;
  padding: 3.5rem 1.25rem 5.5rem;
  color: var(--ink);
  background:
    radial-gradient(circle at 12% 8%, rgba(255, 224, 235, .78), transparent 26rem),
    linear-gradient(180deg, #fffafb 0%, #fff 34%, #fff8fa 100%);
}
.ambient { position: absolute; border-radius: 999px; pointer-events: none; filter: blur(2px); }
.ambient-one { width: 22rem; height: 22rem; top: 28rem; right: -12rem; background: rgba(250, 207, 222, .27); }
.ambient-two { width: 14rem; height: 14rem; top: 62rem; left: -8rem; border: 1px solid rgba(219, 121, 157, .18); }
.terms-hero { position: relative; max-width: 60rem; margin: 0 auto 2.5rem; text-align: center; }
.hero-mark { display: grid; place-items: center; width: 4.75rem; height: 4.75rem; margin: 0 auto 1.15rem; border: 1px solid rgba(210, 105, 146, .24); border-radius: 1.45rem; color: #76947c; background: rgba(255,255,255,.86); box-shadow: 0 18px 45px rgba(160, 75, 110, .12); transform: rotate(-3deg); }
.hero-mark svg { width: 3.25rem; height: 3.25rem; transform: rotate(3deg); }
.eyebrow { margin: 0 0 .75rem; color: var(--rose); font-size: .72rem; font-weight: 800; letter-spacing: .22em; }
.terms-hero h1 { margin: 0; color: #55283c; font-family: Georgia, 'Noto Serif SC', serif; font-size: clamp(2.7rem, 6vw, 4.7rem); font-weight: 500; letter-spacing: .08em; }
.hero-copy { max-width: 45rem; margin: 1rem auto 1.35rem; color: var(--muted); font-size: 1.04rem; line-height: 1.9; }
.hero-meta { display: flex; flex-wrap: wrap; justify-content: center; gap: .55rem; }
.hero-meta span { padding: .48rem .82rem; border: 1px solid #f1d5df; border-radius: 999px; color: #986179; background: rgba(255,255,255,.72); font-size: .75rem; }
.promise-grid { position: relative; z-index: 1; display: grid; grid-template-columns: repeat(3, 1fr); gap: .8rem; max-width: 70rem; margin: 0 auto 2rem; }
.promise-card { display: flex; align-items: center; gap: .85rem; padding: 1rem 1.15rem; border: 1px solid rgba(229, 182, 199, .6); border-radius: 1.15rem; background: rgba(255,255,255,.78); box-shadow: 0 12px 35px rgba(109, 54, 79, .06); backdrop-filter: blur(10px); }
.promise-icon { flex: 0 0 auto; display: grid; place-items: center; width: 2.55rem; height: 2.55rem; border-radius: .85rem; color: var(--rose); background: #fff0f5; }
.promise-icon svg { width: 1.35rem; height: 1.35rem; }
.promise-card strong { display: block; margin-bottom: .18rem; font-size: .92rem; }
.promise-card p { margin: 0; color: var(--muted); font-size: .76rem; line-height: 1.5; }
.terms-layout { position: relative; z-index: 1; display: grid; grid-template-columns: 13.5rem minmax(0, 1fr); align-items: start; gap: 1.35rem; max-width: 70rem; margin: 0 auto; }
.terms-nav { position: sticky; top: 6.4rem; padding: 1.15rem; border: 1px solid #efd9e1; border-radius: 1.25rem; background: rgba(255,255,255,.9); box-shadow: 0 16px 45px rgba(104, 55, 77, .07); }
.nav-label { margin: 0 0 .65rem; padding: 0 .45rem .75rem; border-bottom: 1px solid #f3e4e9; color: #9b6b7f; font-size: .72rem; font-weight: 800; letter-spacing: .14em; }
.terms-nav > a { display: flex; align-items: center; gap: .65rem; padding: .62rem .5rem; border-radius: .65rem; color: #6f5360; font-size: .82rem; text-decoration: none; transition: color .2s ease, background .2s ease, transform .2s ease; }
.terms-nav > a span { color: #cf7899; font-family: Georgia, serif; font-size: .7rem; }
.terms-nav > a:hover, .terms-nav > a:focus-visible { color: var(--rose-dark); background: #fff1f5; transform: translateX(3px); outline: none; }
.nav-help { margin-top: .75rem; padding: .85rem; border-radius: .85rem; background: linear-gradient(135deg, #fff0f5, #fff8f4); }
.nav-help span { display: block; margin-bottom: .32rem; color: #98707f; font-size: .7rem; }
.nav-help a { color: var(--rose-dark); font-size: .78rem; font-weight: 800; }
.terms-content { min-width: 0; }
.notice-card { display: flex; gap: 1rem; margin-bottom: 1rem; padding: 1.15rem 1.25rem; border: 1px solid #f2c9d7; border-radius: 1.15rem; background: linear-gradient(135deg, #fff1f6, #fff9f5); }
.notice-icon { flex: 0 0 auto; display: grid; place-items: center; width: 2rem; height: 2rem; border-radius: 50%; color: #fff; background: var(--rose); font-family: Georgia, serif; font-weight: 700; }
.notice-card strong { display: block; margin-bottom: .32rem; color: #73344f; }
.notice-card p { margin: 0; color: #765c68; font-size: .84rem; line-height: 1.75; }
.term-section { scroll-margin-top: 7rem; margin-bottom: .85rem; padding: 1.6rem 1.75rem; border: 1px solid #f0e2e7; border-radius: 1.25rem; background: rgba(255,255,255,.92); box-shadow: 0 12px 38px rgba(91, 46, 67, .055); transition: border-color .25s ease, box-shadow .25s ease, transform .25s ease; }
.term-section:hover { border-color: #e9c6d2; box-shadow: 0 18px 42px rgba(91, 46, 67, .08); transform: translateY(-2px); }
.section-heading { display: flex; align-items: center; gap: 1rem; margin-bottom: 1rem; }
.section-heading > span { display: grid; place-items: center; width: 2.85rem; height: 2.85rem; border: 1px solid #edc7d5; border-radius: .9rem; color: var(--rose); background: #fff6f8; font-family: Georgia, serif; font-size: .86rem; }
.section-heading p { margin: 0 0 .12rem; color: #bc879b; font-size: .61rem; font-weight: 800; letter-spacing: .14em; text-transform: uppercase; }
.section-heading h2 { margin: 0; color: #5a2b40; font-size: 1.18rem; letter-spacing: .02em; }
.term-section > p { margin: .65rem 0 0; color: #6e5963; font-size: .9rem; line-height: 1.92; }
.term-section ul { display: grid; gap: .58rem; margin: .85rem 0 0; padding: 0; list-style: none; }
.term-section li { position: relative; padding-left: 1.15rem; color: #6e5963; font-size: .88rem; line-height: 1.78; }
.term-section li::before { content: ''; position: absolute; top: .73rem; left: 0; width: .36rem; height: .36rem; border-radius: 50%; background: #dc83a5; box-shadow: 0 0 0 4px #fff0f5; }
.term-section :deep(a) { color: #bd4776; font-weight: 700; text-decoration: none; border-bottom: 1px solid #ebbad0; }
.term-section :deep(a:hover) { color: #852d53; }
.contact-panel { display: grid; grid-template-columns: repeat(3, 1fr); gap: .65rem; margin-top: 1.15rem; }
.contact-panel div { padding: .85rem; border: 1px solid #f0dae2; border-radius: .85rem; background: #fff9fb; }
.contact-panel span { display: block; margin-bottom: .25rem; color: #a77a8c; font-size: .68rem; }
.contact-panel a, .contact-panel strong { color: #6b354d; font-size: .8rem; font-weight: 800; word-break: break-word; }
.terms-end { padding: 2.5rem 1.5rem .5rem; text-align: center; }
.terms-end > div:first-child { display: grid; place-items: center; width: 2.7rem; height: 2.7rem; margin: 0 auto .8rem; border-radius: 50%; color: #c45781; background: #ffe9f1; }
.terms-end h2 { margin: 0; color: #5b2b40; font-family: Georgia, 'Noto Serif SC', serif; font-size: 1.55rem; }
.terms-end p { margin: .55rem 0 1.2rem; color: #957485; font-size: .85rem; }
.end-actions { display: flex; justify-content: center; gap: .65rem; }
.end-actions a { padding: .72rem 1.2rem; border-radius: 999px; font-size: .82rem; font-weight: 800; text-decoration: none; transition: transform .2s ease, box-shadow .2s ease; }
.end-actions a:hover { transform: translateY(-2px); }
.primary-action { color: #fff; background: linear-gradient(135deg, #cf5d8a, #aa3d69); box-shadow: 0 10px 24px rgba(177, 63, 108, .22); }
.secondary-action { border: 1px solid #e7b9cb; color: #9a3c62; background: #fff; }
@media (max-width: 900px) {
  .terms-page { padding-top: 2.5rem; }
  .terms-layout { grid-template-columns: 1fr; }
  .terms-nav { position: relative; top: auto; display: flex; gap: .4rem; overflow-x: auto; padding: .75rem; scrollbar-width: thin; }
  .nav-label, .nav-help { display: none; }
  .terms-nav > a { flex: 0 0 auto; white-space: nowrap; padding: .55rem .7rem; background: #fff7fa; }
}
@media (max-width: 700px) {
  .terms-page { padding: 2rem .85rem 4rem; }
  .promise-grid { grid-template-columns: 1fr; }
  .hero-copy { font-size: .92rem; }
  .hero-meta { gap: .4rem; }
  .hero-meta span { font-size: .68rem; }
  .term-section { padding: 1.3rem 1.1rem; border-radius: 1rem; }
  .section-heading { align-items: flex-start; gap: .75rem; }
  .contact-panel { grid-template-columns: 1fr; }
  .notice-card { padding: 1rem; }
}
@media (prefers-reduced-motion: reduce) {
  *, *::before, *::after { scroll-behavior: auto !important; transition-duration: .01ms !important; }
}
</style>
