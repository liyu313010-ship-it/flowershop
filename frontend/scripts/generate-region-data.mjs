import { mkdir, writeFile } from 'node:fs/promises'
import { dirname, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'
import { areaList } from '@vant/area-data'

const scriptDirectory = dirname(fileURLToPath(import.meta.url))
const outputPath = resolve(scriptDirectory, '../public/regions/pcas-code.json')

const entriesForProvince = (list, provinceCode) => Object.entries(list)
  .filter(([code]) => code.startsWith(provinceCode.slice(0, 2)))

const regionTree = Object.entries(areaList.province_list).map(([provinceCode, provinceName]) => ({
  code: provinceCode,
  name: provinceName,
  children: entriesForProvince(areaList.city_list, provinceCode).map(([cityCode, cityName]) => ({
    code: cityCode,
    name: cityName,
    children: Object.entries(areaList.county_list)
      .filter(([districtCode]) => districtCode.slice(0, 4) === cityCode.slice(0, 4))
      .map(([districtCode, districtName]) => ({
        code: districtCode,
        name: districtName
      }))
  }))
}))

const provinceCount = regionTree.length
const cityCount = regionTree.reduce((total, province) => total + province.children.length, 0)
const districtCount = regionTree.reduce(
  (total, province) => total + province.children.reduce(
    (cityTotal, city) => cityTotal + city.children.length,
    0
  ),
  0
)

if (provinceCount !== 34 || cityCount < 360 || districtCount < 3400) {
  throw new Error(
    `生成的行政区划不完整：${provinceCount} 个省级、${cityCount} 个市级、${districtCount} 个区县级地区`
  )
}

await mkdir(dirname(outputPath), { recursive: true })
await writeFile(outputPath, `${JSON.stringify(regionTree)}\n`, 'utf8')
console.log(`已生成全国行政区划：${provinceCount} 个省级、${cityCount} 个市级、${districtCount} 个区县级地区`)
