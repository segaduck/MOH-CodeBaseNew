# -*- coding: utf-8 -*-
"""
修正所有 .md 檔案的 footer 編碼問題
"""
import os
import re
import codecs

def fix_footer(file_path):
    """修正單一檔案的 footer"""
    try:
        # 讀取檔案內容 (嘗試多種編碼)
        content = None
        for encoding in ['utf-8', 'utf-8-sig', 'cp950', 'gbk']:
            try:
                with codecs.open(file_path, 'r', encoding=encoding) as f:
                    content = f.read()
                break
            except:
                continue
        
        if content is None:
            print(f"  ✗ 無法讀取檔案: {file_path}")
            return False
        
        # 找到最後一個 "---" 的位置
        lines = content.split('\n')
        last_dash_index = -1
        
        for i in range(len(lines) - 1, -1, -1):
            if lines[i].strip() == '---':
                last_dash_index = i
                break
        
        # 如果找到 "---"，保留之前的內容
        if last_dash_index > 0:
            lines = lines[:last_dash_index]
        
        # 移除尾部空行
        while lines and lines[-1].strip() == '':
            lines.pop()
        
        # 加入新的 footer
        new_footer = [
            '',
            '---',
            '',
            '**版本：** 1.0',
            '**日期：** 2025-10-20',
            '**作者：** 柏通股份有限公司',
            ''
        ]
        
        lines.extend(new_footer)
        
        # 寫回檔案 (使用 UTF-8 無 BOM)
        new_content = '\n'.join(lines)
        with codecs.open(file_path, 'w', encoding='utf-8') as f:
            f.write(new_content)
        
        return True
    except Exception as e:
        print(f"  ✗ 錯誤: {str(e)}")
        return False

def main():
    """主程式"""
    base_dir = r'F:\AITest\MOH\CodeBase\109_e-service\技術文件'
    
    # 找出所有 .md 檔案
    md_files = []
    for root, dirs, files in os.walk(base_dir):
        for file in files:
            if file.endswith('.md') and file != 'fix_footer_encoding.py':
                md_files.append(os.path.join(root, file))
    
    print(f'找到 {len(md_files)} 個 .md 檔案')
    print('')
    
    success_count = 0
    fail_count = 0
    
    for file_path in md_files:
        rel_path = os.path.relpath(file_path, base_dir)
        print(f'處理: {rel_path}')
        
        if fix_footer(file_path):
            print(f'  ✓ 成功')
            success_count += 1
        else:
            fail_count += 1
    
    print('')
    print(f'完成! 成功: {success_count}, 失敗: {fail_count}')

if __name__ == '__main__':
    main()

