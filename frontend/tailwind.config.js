/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        // 欢雨鲜花粉色主题
        'huanyu-pink': {
          50: '#fdf2f8',
          100: '#fce7f3',
          200: '#fbcfe8',
          300: '#f9a8d4',
          400: '#f472b6',
          500: '#ec4899',
          600: '#db2777',
          700: '#be185d',
          800: '#9d174d',
          900: '#831843',
        },
        'huanyu-green': {
          50: '#f0fdf4',
          100: '#dcfce7',
          200: '#bbf7d0',
          300: '#86efac',
          400: '#4ade80',
          500: '#22c55e',
        },
        'huanyu-red': {
          50: '#fff1f2',
          100: '#ffe4e6',
          200: '#fecdd3',
          300: '#fda4af',
          400: '#fb7185',
          500: '#f43f5e',
          600: '#e11d48',
          700: '#be123c',
          800: '#9f1239',
          900: '#881337',
        },
        // 渐变色定义
        'gradient-purple': {
          start: '#667eea',
          middle: '#764ba2', 
          end: '#f093fb'
        }
      },
      fontFamily: {
        'sans': ['Noto Sans SC', 'system-ui', 'sans-serif'],
      },
      backgroundImage: {
        'flower-pattern': "url('/images/flower-pattern.svg')"
      }
    }
  },
  plugins: [],
}
