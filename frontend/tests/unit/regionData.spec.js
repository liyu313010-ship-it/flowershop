import { readFileSync } from 'node:fs'
import { resolve } from 'node:path'
import {
  ensureRegionDataLoaded,
  getCities,
  getDistricts,
  getProvinceCodeByName,
  getCityCodeByName,
  getDistrictCodeByName,
  getRegionDataStats
} from '@/utils/regionData'

const regions = JSON.parse(
  readFileSync(resolve(process.cwd(), 'public/regions/pcas-code.json'), 'utf8')
)

const findProvince = code => regions.find(region => region.code === code)
const findCity = (provinceCode, cityCode) => findProvince(provinceCode)?.children
  .find(city => city.code === cityCode)

describe('全国省市区数据', () => {
  beforeAll(async () => {
    global.fetch = jest.fn().mockResolvedValue({
      ok: true,
      json: async () => regions
    })
    await ensureRegionDataLoaded()
  })

  test('包含全部省级、市级和区县级行政区', () => {
    const cities = regions.flatMap(province => province.children || [])
    const districts = cities.flatMap(city => city.children || [])

    expect(regions).toHaveLength(34)
    expect(cities.length).toBeGreaterThanOrEqual(360)
    expect(districts.length).toBeGreaterThanOrEqual(3400)
  })

  test('云南省曲靖市包含完整区县，不再只有麒麟区', () => {
    const qujing = findCity('530000', '530300')
    const districtNames = qujing.children.map(district => district.name)

    expect(districtNames).toEqual(expect.arrayContaining([
      '麒麟区', '沾益区', '马龙区', '陆良县', '师宗县',
      '罗平县', '富源县', '会泽县', '宣威市'
    ]))
  })

  test('包含台湾、香港和澳门的三级地址', () => {
    expect(findProvince('710000').children).toHaveLength(22)
    expect(findCity('810000', '810100').children.length).toBeGreaterThan(0)
    expect(findCity('820000', '820100').children.length).toBeGreaterThan(0)
  })

  test('三级联动和历史地址名称回显可用', () => {
    expect(getRegionDataStats()).toEqual({
      provinces: 34,
      cities: 369,
      districts: 3479
    })
    expect(getCities('530000').map(city => city.name)).toContain('曲靖市')
    expect(getDistricts('530000', '530300').map(district => district.name))
      .toContain('宣威市')
    expect(getProvinceCodeByName('云南省')).toBe('530000')
    expect(getCityCodeByName('530000', '曲靖市')).toBe('530300')
    expect(getDistrictCodeByName('530000', '530300', '麒麟区')).toBe('530302')
  })
})
