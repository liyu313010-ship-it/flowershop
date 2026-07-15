/**
 * 从接口返回值中提取集合，兼容 axios 原始响应、已解包响应和分页响应。
 */
export const getCollectionItems = (response) => {
  const payload = response?.data ?? response
  if (Array.isArray(payload)) return payload
  const items = payload?.items ?? payload?.Items
  return Array.isArray(items) ? items : []
}
