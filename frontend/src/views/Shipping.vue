<template>
  <div class="min-h-screen bg-gray-50 py-8">
    <div class="container mx-auto px-4">
      <h1 class="text-3xl font-bold text-gray-800 mb-8">配送说明</h1>
      
      <div class="bg-white rounded-lg shadow-lg p-8 mb-8">
        <h2 class="text-2xl font-bold text-pink-600 mb-4">配送范围</h2>
        <p class="text-gray-700 leading-relaxed mb-4">
          我们目前提供全国范围内的鲜花配送服务，覆盖所有省市自治区。对于特殊偏远地区，可能需要额外的配送时间。
        </p>
        <div class="grid md:grid-cols-2 gap-8 mt-6">
          <div class="bg-gray-50 p-6 rounded-lg">
            <h3 class="text-xl font-semibold mb-3 text-pink-500">同城配送</h3>
            <p class="text-gray-600">主城区内提供当日达服务，郊区可在次日送达。</p>
          </div>
          <div class="bg-gray-50 p-6 rounded-lg">
            <h3 class="text-xl font-semibold mb-3 text-pink-500">异地配送</h3>
            <p class="text-gray-600">全国各大城市3日内送达，偏远地区5日内送达。</p>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow-lg p-8 mb-8">
        <h2 class="text-2xl font-bold text-pink-600 mb-4">配送时间</h2>
        <div class="overflow-x-auto">
          <table class="min-w-full divide-y divide-gray-200">
            <thead>
              <tr>
                <th class="px-6 py-3 bg-gray-50 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">配送类型</th>
                <th class="px-6 py-3 bg-gray-50 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">配送时间</th>
                <th class="px-6 py-3 bg-gray-50 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">订单截止时间</th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr>
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">当日达</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">10:00-18:00</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">当天12:00前</td>
              </tr>
              <tr>
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">次日达</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">10:00-18:00</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">前一天20:00前</td>
              </tr>
              <tr>
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">定时配送</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">指定时间段</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">提前24小时</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow-lg p-8 mb-8">
        <h2 class="text-2xl font-bold text-pink-600 mb-4">配送费用</h2>
        <ul class="space-y-3 text-gray-700">
          <li class="flex items-center"><span class="w-3 h-3 bg-pink-500 rounded-full mr-3"></span>订单金额满299元，全国免费配送</li>
          <li class="flex items-center"><span class="w-3 h-3 bg-pink-500 rounded-full mr-3"></span>订单金额199-298元，收取10元配送费</li>
          <li class="flex items-center"><span class="w-3 h-3 bg-pink-500 rounded-full mr-3"></span>订单金额198元以下，收取20元配送费</li>
          <li class="flex items-center"><span class="w-3 h-3 bg-pink-500 rounded-full mr-3"></span>定时配送额外收取15元服务费</li>
        </ul>
      </div>

      <div class="bg-white rounded-lg shadow-lg p-8">
        <h2 class="text-2xl font-bold text-pink-600 mb-4">配送注意事项</h2>
        <div class="space-y-4 text-gray-700">
          <p>1. 节假日期间可能出现配送延迟，请提前下单。</p>
          <p>2. 收到鲜花后，请立即拆开包装，根据说明进行养护。</p>
          <p>3. 如需更改配送地址或时间，请至少提前4小时联系客服。</p>
          <p>4. 鲜花属于生鲜产品，配送过程中可能会有轻微损伤，敬请谅解。</p>
          <p>5. 如遇特殊天气（暴雨、暴雪等）影响配送，我们将提前联系您协商解决方案。</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'Shipping'
}
</script>

<style scoped>
.container {
  max-width: 1200px;
}

/* 添加表格响应式样式 */
@media (max-width: 768px) {
  table {
    font-size: 14px;
  }
  .px-6 {
    padding-left: 1rem;
    padding-right: 1rem;
  }
  .py-4 {
    padding-top: 0.75rem;
    padding-bottom: 0.75rem;
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
</style>
