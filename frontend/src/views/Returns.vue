<template>
  <div class="min-h-screen bg-gray-50 py-8">
    <div class="container mx-auto px-4">
      <h1 class="text-3xl font-bold text-gray-800 mb-8">退换货政策</h1>
      
      <div class="bg-white rounded-lg shadow-lg p-8 mb-8">
        <h2 class="text-2xl font-bold text-pink-600 mb-4">退换货原则</h2>
        <p class="text-gray-700 leading-relaxed mb-6">
          欢雨flower坚持以顾客满意度为首要原则，我们理解鲜花作为特殊商品的性质，特制定以下公平合理的退换货政策：
        </p>
        <div class="grid md:grid-cols-2 gap-6">
          <div class="border-l-4 border-pink-500 pl-4">
            <h3 class="text-xl font-semibold mb-2 text-pink-500">退货条件</h3>
            <ul class="list-disc pl-5 space-y-2 text-gray-700">
              <li>收到的鲜花与订单描述严重不符</li>
              <li>鲜花在配送过程中受损严重（超过30%的花朵枯萎）</li>
              <li>由于我们的原因导致花材错误或数量短缺</li>
              <li>订单配送错误（送到错误地址或错误收货人）</li>
            </ul>
          </div>
          <div class="border-l-4 border-purple-500 pl-4">
            <h3 class="text-xl font-semibold mb-2 text-purple-500">换货条件</h3>
            <ul class="list-disc pl-5 space-y-2 text-gray-700">
              <li>收到的鲜花品质不符合预期（可提供同等价位的替换）</li>
              <li>部分花材因季节性原因需要替换，但未提前告知</li>
              <li>包装与展示图片有明显差异影响整体效果</li>
              <li>由于天气原因导致轻微受损（可更换新鲜花材）</li>
            </ul>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow-lg p-8 mb-8">
        <h2 class="text-2xl font-bold text-pink-600 mb-4">退换货流程</h2>
        <div class="relative">
          <!-- 流程时间线 -->
          <div class="hidden md:block absolute left-1/2 transform -translate-x-1/2 h-full w-0.5 bg-gray-200"></div>
          <!-- 步骤1 -->
          <div class="flex flex-col md:flex-row items-center mb-8 relative">
            <div class="flex items-center justify-center w-12 h-12 rounded-full bg-pink-500 text-white font-bold text-xl z-10 mb-4 md:mb-0">1</div>
            <div class="md:ml-8 md:w-1/2 text-center md:text-left">
              <h3 class="text-xl font-semibold mb-2 text-gray-800">拍照留证并联系客服</h3>
              <p class="text-gray-600">收到鲜花后，如发现问题，请在2小时内拍照记录并联系我们的客服。</p>
            </div>
          </div>
          <!-- 步骤2 -->
          <div class="flex flex-col md:flex-row items-center mb-8 relative">
            <div class="flex items-center justify-center w-12 h-12 rounded-full bg-pink-500 text-white font-bold text-xl z-10 mb-4 md:mb-0">2</div>
            <div class="md:ml-8 md:w-1/2 text-center md:text-left">
              <h3 class="text-xl font-semibold mb-2 text-gray-800">提交申请并提供凭证</h3>
              <p class="text-gray-600">通过客服渠道提交退换货申请，提供订单号、问题描述和照片凭证。</p>
            </div>
          </div>
          <!-- 步骤3 -->
          <div class="flex flex-col md:flex-row items-center mb-8 relative">
            <div class="flex items-center justify-center w-12 h-12 rounded-full bg-pink-500 text-white font-bold text-xl z-10 mb-4 md:mb-0">3</div>
            <div class="md:ml-8 md:w-1/2 text-center md:text-left">
              <h3 class="text-xl font-semibold mb-2 text-gray-800">客服审核确认问题</h3>
              <p class="text-gray-600">我们的客服将在2小时内审核您的申请，确认问题是否符合退换货条件。</p>
            </div>
          </div>
          <!-- 步骤4 -->
          <div class="flex flex-col md:flex-row items-center relative">
            <div class="flex items-center justify-center w-12 h-12 rounded-full bg-pink-500 text-white font-bold text-xl z-10 mb-4 md:mb-0">4</div>
            <div class="md:ml-8 md:w-1/2 text-center md:text-left">
              <h3 class="text-xl font-semibold mb-2 text-gray-800">处理退换货并完成</h3>
              <p class="text-gray-600">审核通过后，我们将根据您的选择安排退货退款或重新配送替换产品。</p>
            </div>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow-lg p-8 mb-8">
        <h2 class="text-2xl font-bold text-pink-600 mb-4">退款说明</h2>
        <div class="space-y-4 text-gray-700">
          <p>1. 确认符合退货条件后，我们将在24小时内处理退款申请。</p>
          <p>2. 退款将原路返回您的支付账户，到账时间取决于您的支付方式（一般为1-7个工作日）。</p>
          <p>3. 如使用优惠券支付的订单，退还的金额将扣除优惠券部分。</p>
          <p>4. 配送费用：因我方原因导致的退货，全额退款（含配送费）；因顾客个人原因的退货，扣除配送费用。</p>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow-lg p-8">
        <h2 class="text-2xl font-bold text-pink-600 mb-4">特殊说明</h2>
        <div class="bg-yellow-50 border-l-4 border-yellow-400 p-4 mb-6">
          <p class="text-yellow-700">⚠️ 注意：鲜花作为生鲜易腐商品，以下情况不支持退换货：</p>
          <ul class="list-disc pl-5 mt-2 space-y-1 text-yellow-700">
            <li>因正常养护不当导致的鲜花枯萎</li>
            <li>超过2小时未提出异议的情况</li>
            <li>因个人审美差异对花束造型不满意
            </li>
            <li>节假日、特殊活动期间不支持无理由退换</li>
          </ul>
        </div>
        <div class="text-gray-700">
          <p class="mb-4">• 季节性花材替换：由于鲜花的季节性特点，部分花材可能会被同等价位、相似颜色和风格的花材替代，这不属于质量问题。</p>
          <p class="mb-4">• 生日、纪念日等特殊场合订单：建议至少提前24小时下单，并在收货时当场检查，确保及时处理可能出现的问题。</p>
          <p>• 如有疑问，请联系客服电话：15025033613 或邮箱：2784107771@qq.com</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'Returns'
}
</script>

<style scoped>
.container {
  max-width: 1200px;
}

/* 添加响应式样式 */
@media (max-width: 768px) {
  h1 {
    font-size: 2xl;
  }
  h2 {
    font-size: 1.5rem;
  }
  .grid.md\:grid-cols-2 {
    grid-template-columns: 1fr;
  }
}

/* 添加平滑过渡效果 */
h1, h2, h3 {
  transition: color 0.3s ease;
}

h2:hover {
  color: #e02f6b;
}

/* 卡片悬停效果 */
.bg-white.rounded-lg {
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.bg-white.rounded-lg:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
}

/* 时间线样式优化 */
@media (min-width: 768px) {
  .w-1\/2:last-child {
    margin-left: 50%;
  }
}

/* 圆形数字按钮样式 */
.rounded-full {
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
  transition: all 0.3s ease;
}

.rounded-full:hover {
  transform: scale(1.05);
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
}
</style>
