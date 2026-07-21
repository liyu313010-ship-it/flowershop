const regionData = {}
let loaded = false
let loadingPromise = null

const FULL_REGION_DATA_URL = '/regions/pcas-code.json?v=2025.08'
const MIN_PROVINCES = 34
const MIN_CITIES = 360
const MIN_DISTRICTS = 3400

const normalize = value => String(value || '').trim()

function getProvince(provinceCode) {
  return regionData[normalize(provinceCode)]
}

function getCity(provinceCode, cityCode) {
  return getProvince(provinceCode)?.cities?.[normalize(cityCode)]
}

function countRegions(data = regionData) {
  const provinces = Object.values(data)
  const cities = provinces.flatMap(province => Object.values(province.cities || {}))
  return {
    provinces: provinces.length,
    cities: cities.length,
    districts: cities.reduce(
      (total, city) => total + Object.keys(city.districts || {}).length,
      0
    )
  }
}

function hasCompleteRegionData(data = regionData) {
  const counts = countRegions(data)
  return counts.provinces >= MIN_PROVINCES &&
    counts.cities >= MIN_CITIES &&
    counts.districts >= MIN_DISTRICTS
}

function clearRegionData() {
  Object.keys(regionData).forEach(code => delete regionData[code])
}

function mergePcas(pcas) {
  if (!Array.isArray(pcas)) {
    throw new TypeError('行政区划数据格式不正确')
  }

  const nextData = {}
  pcas.forEach(province => {
    if (!province?.code || !province?.name) return
    const cities = {}

    ;(province.children || []).forEach(city => {
      if (!city?.code || !city?.name) return
      const districts = {}
      ;(city.children || []).forEach(district => {
        if (district?.code && district?.name) {
          districts[String(district.code)] = { name: district.name }
        }
      })
      cities[String(city.code)] = { name: city.name, districts }
    })

    nextData[String(province.code)] = { name: province.name, cities }
  })

  if (!hasCompleteRegionData(nextData)) {
    const counts = countRegions(nextData)
    throw new Error(
      `行政区划数据不完整：${counts.provinces} 个省级、${counts.cities} 个市级、${counts.districts} 个区县级地区`
    )
  }

  clearRegionData()
  Object.assign(regionData, nextData)
}

export function getProvinces() {
  return Object.entries(regionData).map(([code, province]) => ({
    code,
    name: province.name
  }))
}

export function getCities(provinceCode) {
  return Object.entries(getProvince(provinceCode)?.cities || {}).map(([code, city]) => ({
    code,
    name: city.name
  }))
}

export function getDistricts(provinceCode, cityCode) {
  return Object.entries(getCity(provinceCode, cityCode)?.districts || {}).map(([code, district]) => ({
    code,
    name: district.name
  }))
}

export function getProvinceName(provinceCode) {
  return getProvince(provinceCode)?.name || ''
}

export function getCityName(provinceCode, cityCode) {
  return getCity(provinceCode, cityCode)?.name || ''
}

export function getDistrictName(provinceCode, cityCode, districtCode) {
  return getCity(provinceCode, cityCode)?.districts?.[normalize(districtCode)]?.name || ''
}

export function getRegionName(provinceCode, cityCode, districtCode) {
  return [
    getProvinceName(provinceCode),
    getCityName(provinceCode, cityCode),
    getDistrictName(provinceCode, cityCode, districtCode)
  ].join('')
}

export function getProvinceCodeByName(nameOrCode) {
  const value = normalize(nameOrCode)
  if (regionData[value]) return value
  return Object.keys(regionData).find(code => regionData[code].name === value) || ''
}

export function getCityCodeByName(provinceCode, nameOrCode) {
  const value = normalize(nameOrCode)
  const province = getProvince(provinceCode)
  if (!province) return ''
  if (province.cities?.[value]) return value

  const exactCode = Object.keys(province.cities || {})
    .find(code => province.cities[code].name === value)
  if (exactCode) return exactCode

  // 兼容旧地址中直辖市保存为“市辖区”或“县”的情况。
  if (['市辖区', '县', province.name].includes(value)) {
    return Object.keys(province.cities || {})[0] || ''
  }
  return ''
}

export function getDistrictCodeByName(provinceCode, cityCode, nameOrCode) {
  const value = normalize(nameOrCode)
  const districts = getCity(provinceCode, cityCode)?.districts || {}
  if (districts[value]) return value
  return Object.keys(districts).find(code => districts[code].name === value) || ''
}

export async function ensureRegionDataLoaded() {
  if (loaded && hasCompleteRegionData()) return
  if (loadingPromise) return loadingPromise

  loadingPromise = (async () => {
    const response = await fetch(FULL_REGION_DATA_URL, { cache: 'no-cache' })
    if (!response.ok) {
      throw new Error(`加载全国行政区划失败（HTTP ${response.status}）`)
    }
    mergePcas(await response.json())
    loaded = true
  })()

  try {
    await loadingPromise
  } catch (error) {
    loaded = false
    clearRegionData()
    console.error('全国行政区划数据加载失败:', error)
    throw error
  } finally {
    loadingPromise = null
  }
}

export function getRegionDataStats() {
  return countRegions()
}
