module.exports = {
  testEnvironment: 'jest-environment-jsdom',
  moduleFileExtensions: ['js', 'json', 'vue'],
  transform: {
    '^.+\\.vue$': '@vue/vue3-jest',
    '^.+\\.js$': 'babel-jest'
  },
  moduleNameMapper: {
    '^@/(.*)$': '<rootDir>/src/$1'
  },
  testMatch: ['<rootDir>/tests/unit/**/*.spec.(js|vue)'],
  collectCoverage: false,
  collectCoverageFrom: ['src/**/*.{vue,js}', '!src/main.js', '!src/router/index.js', '!src/stores/*.js', '!**/node_modules/**'],
  coverageDirectory: '<rootDir>/tests/coverage'
}
