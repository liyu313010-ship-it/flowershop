// 区域数据存储对象 - 现在包含完整的全国行政区划数据
const regionData = {
  // 省份数据将在initRegionData中初始化
  // 包含全部34个省级行政区及下属市县级数据
};
let loaded = false;

// 注意: 完整的全国行政区划数据量较大(约数千条记录)，
// 实际应用中可以按需加载或使用动态加载机制

// 获取所有省份 - 支持树形结构数据
export function getProvinces() {
  if (Object.keys(regionData).length === 0) {

  }
  return Object.keys(regionData).map(code => ({
    code,
    name: regionData[code].name
  }));
}

// 根据省份代码获取城市 - 支持树形结构数据
export function getCities(provinceCode) {
  if (!regionData[provinceCode]) {
    return [];
  }
  
  // 支持两种数据结构: 1) 直接cities对象 2) children数组
  const citiesObj = regionData[provinceCode].cities;
  const citiesArray = regionData[provinceCode].children;
  
  if (citiesObj) {
    return Object.keys(citiesObj).map(code => ({
      code,
      name: citiesObj[code].name
    }));
  } else if (citiesArray) {
    return citiesArray.map(city => ({
      code: city.code,
      name: city.name
    }));
  }
  
  return [];
}

// 根据省份和城市代码获取区县 - 支持树形结构数据
export function getDistricts(provinceCode, cityCode) {
  if (!regionData[provinceCode]) {
    return [];
  }
  
  // 支持两种数据结构: 1) 直接对象嵌套 2) children数组
  const province = regionData[provinceCode];
  
  // 尝试从cities对象中获取
  if (province.cities && province.cities[cityCode]) {
    const districtsObj = province.cities[cityCode].districts;
    if (districtsObj) {
      return Object.keys(districtsObj).map(code => ({
        code,
        name: districtsObj[code].name
      }));
    }
  }
  
  // 尝试从children数组中获取
  if (province.children) {
    const city = province.children.find(c => c.code === cityCode);
    if (city && city.children) {
      return city.children.map(district => ({
        code: district.code,
        name: district.name
      }));
    }
  }
  
  return [];
}

// 根据代码获取区域名称 - 支持树形结构数据
export function getRegionName(provinceCode, cityCode, districtCode) {
  // 检查数据是否已加载
  if (!loaded) {

  }
  
  let name = '';
  
  if (regionData[provinceCode]) {
    name = regionData[provinceCode].name;
    
    // 支持两种数据结构：对象嵌套和children数组
    if (cityCode) {
      // 尝试从cities对象获取
      let cityName = '';
      if (regionData[provinceCode].cities && regionData[provinceCode].cities[cityCode]) {
        cityName = regionData[provinceCode].cities[cityCode].name;
      } 
      // 尝试从children数组获取
      else if (regionData[provinceCode].children) {
        const city = regionData[provinceCode].children.find(c => c.code === cityCode);
        cityName = city?.name || '';
      }
      
      if (cityName) {
        name += cityName;
        
        if (districtCode) {
          // 尝试从对象结构获取区县
          let districtName = '';
          if (regionData[provinceCode].cities && 
              regionData[provinceCode].cities[cityCode] && 
              regionData[provinceCode].cities[cityCode].districts && 
              regionData[provinceCode].cities[cityCode].districts[districtCode]) {
            districtName = regionData[provinceCode].cities[cityCode].districts[districtCode].name;
          }
          // 尝试从children数组获取区县
          else if (regionData[provinceCode].children) {
            const city = regionData[provinceCode].children.find(c => c.code === cityCode);
            if (city?.children) {
              const district = city.children.find(d => d.code === districtCode);
              districtName = district?.name || '';
            }
          }
          
          if (districtName) {
            name += districtName;
          }
        }
      }
    }
  }
  
  return name;
}

// 仅获取省名称 - 支持树形结构数据
export function getProvinceName(provinceCode) {
  // 检查数据是否已加载
  if (!loaded) {

  }
  return regionData[provinceCode]?.name || ''
}

// 仅获取市名称 - 支持树形结构数据
export function getCityName(provinceCode, cityCode) {
  const prov = regionData[provinceCode]
  if (!prov) return ''
  
  // 尝试从cities对象获取
  if (prov.cities && prov.cities[cityCode]) {
    return prov.cities[cityCode].name || '';
  }
  
  // 尝试从children数组获取
  if (prov.children) {
    const city = prov.children.find(c => c.code === cityCode);
    return city?.name || '';
  }
  
  return ''
}

// 仅获取区/县名称 - 支持树形结构数据
export function getDistrictName(provinceCode, cityCode, districtCode) {
  const prov = regionData[provinceCode]
  if (!prov) return ''
  
  // 尝试从对象结构获取区县
  if (prov.cities && prov.cities[cityCode] && prov.cities[cityCode].districts) {
    return prov.cities[cityCode].districts[districtCode]?.name || '';
  }
  
  // 尝试从children数组获取区县
  if (prov.children) {
    const city = prov.children.find(c => c.code === cityCode);
    if (city?.children) {
      const district = city.children.find(d => d.code === districtCode);
      return district?.name || '';
    }
  }
  
  return ''
}

// 动态加载完整数据（优先从本地 /regions.json，其次远程）
export async function ensureRegionDataLoaded() {
  if (loaded) {

    return;
  }
  
  let dataSource = '未知';
  let provinceCount = 0;
  
  // 优先使用前端本地完整数据文件（public/regions/pcas-code.json）
  try {

    const respLocalFull = await fetch('/regions/pcas-code.json');
    if (respLocalFull.ok) {
      const jsonLocalFull = await respLocalFull.json();
      mergePcas(jsonLocalFull);
      provinceCount = Object.keys(regionData).length;
      loaded = true;
      dataSource = 'pcas-code.json';

      return;
    }
  } catch (error) {

  }
  
  // 次选使用后端接口的完整数据（仅在本地文件不可用时尝试）
  try {

    const respFull = await fetch('/api/Regions/pcas');
    if (respFull.ok) {
      const jsonFull = await respFull.json();
      mergePcas(jsonFull);
      provinceCount = Object.keys(regionData).length;
      loaded = true;
      dataSource = 'API';

      return;
    }
  } catch (error) {

  }

  // 再次回退使用前端本地简版文件
  try {

    const resp = await fetch('/regions.json');
    if (resp.ok) {
      const json = await resp.json();
      mergePcas(json);
      provinceCount = Object.keys(regionData).length;
      if (provinceCount >= 31) {
        loaded = true;
        dataSource = 'regions.json';

        return;
      }
    }
  } catch (error) {

  }
  
  // 尝试从另一个可能的路径
  try {

    const respAlt = await fetch('/public/regions/regions.json');
    if (respAlt.ok) {
      const jsonAlt = await respAlt.json();
      mergePcas(jsonAlt);
      provinceCount = Object.keys(regionData).length;
      if (provinceCount >= 31) {
        loaded = true;
        dataSource = 'public/regions/regions.json';

        return;
      }
    }
  } catch (error) {

  }
  
  // 如果所有加载方式都失败，使用默认数据
  try {

    useDefaultRegionData();
    provinceCount = Object.keys(regionData).length;
    loaded = true;
    dataSource = '默认数据';

  } catch (error) {

  }
  
  // 最终检查
  provinceCount = Object.keys(regionData).length;
  if (provinceCount >= 31) {
    loaded = true;

  } else {

  }
}

function mergePcas(pcas) {
  // pcas: [{code,name,children:[{code,name,children:[{code,name}]}]}]
  regionDataClear()
  pcas.forEach(p => {
    regionData[p.code] = { name: p.name, cities: {} }
    ;(p.children || []).forEach(c => {
      regionData[p.code].cities[c.code] = { name: c.name, districts: {} }
      ;(c.children || []).forEach(d => {
        regionData[p.code].cities[c.code].districts[d.code] = { name: d.name }
      })
    })
  })
}

function regionDataClear() {
  Object.keys(regionData).forEach(k => delete regionData[k])
}

// 初始化区域数据 - 异步加载完整的全国行政区划数据
async function initRegionData() {
  try {
    // 尝试从public目录加载regions.json - 包含完整的全国34个省级行政区数据
    const response = await fetch('/regions.json');
    if (!response.ok) {
      throw new Error('Failed to load regions data');
    }
    const regions = await response.json();
    
    // 初始化区域数据，将完整的树形结构存储到regionData中
    regions.forEach(province => {
      // 存储完整的省份数据，包括城市和区县信息
      regionData[province.code] = province;
    });
    

  } catch (error) {

    // 回退到使用默认数据结构
    useDefaultRegionData();
  }
}

// 省份名称映射表 - 全局变量
const provinceNames = {
  "110000": "北京市", "120000": "天津市", "130000": "河北省", "140000": "山西省", 
  "150000": "内蒙古自治区", "210000": "辽宁省", "220000": "吉林省", 
  "230000": "黑龙江省", "310000": "上海市", "320000": "江苏省", "330000": "浙江省", 
  "340000": "安徽省", "350000": "福建省", "360000": "江西省", 
  "370000": "山东省", "410000": "河南省", "420000": "湖北省", 
  "430000": "湖南省", "440000": "广东省", "450000": "广西壮族自治区", "460000": "海南省", 
  "500000": "重庆市", "510000": "四川省", "520000": "贵州省", 
  "530000": "云南省", "540000": "西藏自治区", "610000": "陕西省", "620000": "甘肃省", 
  "630000": "青海省", "640000": "宁夏回族自治区", "650000": "新疆维吾尔自治区", 
  "710000": "台湾省", "810000": "香港特别行政区", "820000": "澳门特别行政区"
};

// 默认数据初始化函数 - 当regions.json加载失败时使用
export function useDefaultRegionData() {
  // 创建基本省份结构作为后备数据
  const defaultProvinces = {
    "110000": { name: "北京市", cities: {} },
    "310000": { name: "上海市", cities: {} },
    "440000": { name: "广东省", cities: {} },
    "530000": { name: "云南省", cities: {} }
  };
  
  // 添加其余省份的基本结构
  const otherProvinces = [
    "120000", "130000", "140000", "150000", "210000", "220000", "230000", 
    "320000", "330000", "340000", "350000", "360000", "370000", "410000", 
    "420000", "430000", "450000", "460000", "500000", "510000", "520000", 
    "540000", "610000", "620000", "630000", "640000", "650000", "710000", 
    "810000", "820000"
  ];
  
  otherProvinces.forEach(code => {
    defaultProvinces[code] = { name: provinceNames[code], cities: {} };
  });
  
  Object.assign(regionData, defaultProvinces);

}
