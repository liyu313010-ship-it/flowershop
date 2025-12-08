const fs = require('fs');
const path = require('path');

// 读取Profile.vue文件
const profilePath = path.join(__dirname, 'frontend', 'src', 'views', 'user', 'Profile.vue');
const content = fs.readFileSync(profilePath, 'utf8');

// 检查文件的基本结构
console.log('=== Profile.vue File Check ===');
console.log('File size:', content.length, 'bytes');

// 检查script标签
const scriptStart = content.indexOf('<script setup>');
const scriptEnd = content.indexOf('</script>', scriptStart);

if (scriptStart === -1) {
  console.error('ERROR: No <script setup> tag found!');
} else if (scriptEnd === -1) {
  console.error('ERROR: No closing </script> tag found!');
} else {
  console.log('✓ Script tags found correctly');
  
  // 提取script内容
  const scriptContent = content.substring(scriptStart + '<script setup>'.length, scriptEnd);
  console.log('Script content length:', scriptContent.length, 'characters');
  
  // 检查基本语法错误
  const lines = scriptContent.split('\n');
  
  // 检查括号匹配
  let braceCount = 0;
  let bracketCount = 0;
  let parenthesisCount = 0;
  
  for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    const lineNumber = i + 1;
    
    // 跳过注释行
    if (line.trim().startsWith('//') || line.trim().startsWith('/*') || line.trim().endsWith('*/')) {
      continue;
    }
    
    // 检查括号
    for (const char of line) {
      if (char === '{') braceCount++;
      if (char === '}') braceCount--;
      if (char === '[') bracketCount++;
      if (char === ']') bracketCount--;
      if (char === '(') parenthesisCount++;
      if (char === ')') parenthesisCount--;
    }
    
    // 检查常见语法错误
    if (line.includes('const') || line.includes('let') || line.includes('var')) {
      // 简单检查变量声明
      if (line.includes('=')) {
        const parts = line.split('=');
        if (parts.length > 2) {
          console.warn(`WARNING: Multiple assignment operators on line ${lineNumber}: ${line.trim()}`);
        }
      }
    }
    
    // 检查函数定义
    if (line.includes('function') || line.includes('=>')) {
      if (!line.includes('{') && !line.trim().endsWith(';')) {
        console.warn(`WARNING: Function definition without opening brace or semicolon on line ${lineNumber}: ${line.trim()}`);
      }
    }
  }
  
  console.log('\n=== Bracket Balance Check ===');
  console.log('Braces {}:', braceCount === 0 ? '✓ Balanced' : `✗ Unbalanced (${braceCount})`);
  console.log('Brackets []:', bracketCount === 0 ? '✓ Balanced' : `✗ Unbalanced (${bracketCount})`);
  console.log('Parentheses ():', parenthesisCount === 0 ? '✓ Balanced' : `✗ Unbalanced (${parenthesisCount})`);
  
  // 检查第1077行附近的代码（根据错误信息）
  console.log('\n=== Line 1077附近代码检查 ===');
  const errorLine = 1077;
  const startLine = Math.max(0, errorLine - 20);
  const endLine = Math.min(lines.length, errorLine + 10);
  
  for (let i = startLine; i < endLine; i++) {
    const lineNumber = i + 1;
    const marker = lineNumber === errorLine ? '>>> ' : '    ';
    console.log(`${marker}${lineNumber}: ${lines[i]}`);
  }
}

// 检查template标签
const templateStart = content.indexOf('<template>');
const templateEnd = content.indexOf('</template>', templateStart);

if (templateStart === -1) {
  console.error('ERROR: No <template> tag found!');
} else if (templateEnd === -1) {
  console.error('ERROR: No closing </template> tag found!');
} else {
  console.log('✓ Template tags found correctly');
}

console.log('\n=== Check Complete ===');
