const fs = require('fs');
const path = require('path');

// 读取Profile.vue文件
const profilePath = path.join(__dirname, 'frontend', 'src', 'views', 'user', 'Profile.vue');
const content = fs.readFileSync(profilePath, 'utf8');

// 提取script标签内容
const scriptRegex = /<script setup>([\s\S]*?)<\/script>/g;
const matches = content.matchAll(scriptRegex);

let scriptContent = '';
for (const match of matches) {
  scriptContent = match[1];
  break;
}

console.log('Script content length:', scriptContent.length);

// 尝试使用Babel解析
const { parse } = require('@babel/parser');
try {
  const ast = parse(scriptContent, {
    sourceType: 'module',
    plugins: ['jsx', 'typescript']
  });
  console.log('Script content is valid JavaScript!');
} catch (error) {
  console.error('Syntax error in script content:', error.message);
  console.error('Error position:', error.loc);
  
  // 打印错误位置附近的代码
  const lines = scriptContent.split('\n');
  const errorLine = error.loc.line;
  const startLine = Math.max(0, errorLine - 5);
  const endLine = Math.min(lines.length, errorLine + 5);
  
  console.error('\nError context:');
  for (let i = startLine; i < endLine; i++) {
    const lineNumber = i + 1;
    const marker = lineNumber === errorLine ? '>>> ' : '    ';
    console.error(`${marker}${lineNumber}: ${lines[i]}`);
  }
}
