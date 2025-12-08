module.exports = {
  preset: '@vue/vue3-jest',
  testEnvironment: 'jest-environment-jsdom',
  moduleFileExtensions: ['js', 'json', 'vue'],
  transform: {
    '^.+\\.vue$': '@vue/vue3-jest',
    '^.+\\.js$': 'babel-jest'
  },
  moduleNameMapping: {
    '^@/(.*)$': '<rootDir>/src/$1'
  },
  testMatch: ['<rootDir>/tests/unit/**/*.spec.(js|vue)'],
  collectCoverage: true,
  collectCoverageFrom: ['src/**/*.{vue,js}', '!src/main.js', '!src/router/index.js', '!src/stores/*.js', '!**/node_modules/**'],
  coverageDirectory: '<rootDir>/tests/coverage'
}