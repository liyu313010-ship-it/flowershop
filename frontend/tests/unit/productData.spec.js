import { getCollectionItems } from '@/utils/apiResponse'
import { getProductImageUrl } from '@/utils/avatar'

describe('product API data normalization', () => {
  const items = [{ id: 1 }, { id: 2 }]

  it('accepts a direct array', () => {
    expect(getCollectionItems(items)).toEqual(items)
  })

  it('extracts items from an unwrapped paged response', () => {
    expect(getCollectionItems({ items, totalCount: 58 })).toEqual(items)
  })

  it('extracts items from an axios paged response', () => {
    expect(getCollectionItems({ data: { items, totalCount: 58 } })).toEqual(items)
  })

  it('returns an empty array for malformed data', () => {
    expect(getCollectionItems({ totalCount: 58 })).toEqual([])
  })
})

describe('product image URL normalization', () => {
  it('removes an invalid API prefix from catalog images', () => {
    expect(getProductImageUrl('/api/images/catalog/four-season-garden.webp'))
      .toBe('/images/catalog/four-season-garden.webp?v=local')
  })

  it('keeps static and uploaded image paths on the web origin', () => {
    expect(getProductImageUrl('/images/catalog/daisy-sunshine.webp'))
      .toBe('/images/catalog/daisy-sunshine.webp?v=local')
    expect(getProductImageUrl('/uploads/products/example.webp'))
      .toBe('/uploads/products/example.webp?v=local')
  })

  it('preserves existing query parameters and does not version CDN images twice', () => {
    expect(getProductImageUrl('/uploads/products/example.webp?size=large'))
      .toBe('/uploads/products/example.webp?size=large&v=local')
    expect(getProductImageUrl('/uploads/products/example.webp?v=release-1'))
      .toBe('/uploads/products/example.webp?v=release-1')
    expect(getProductImageUrl('https://cdn.example.com/example.webp'))
      .toBe('https://cdn.example.com/example.webp')
  })
})
