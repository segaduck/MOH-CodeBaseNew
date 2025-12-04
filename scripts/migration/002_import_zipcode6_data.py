#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
ZIPCODE6 資料匯入腳本
版本: 1.1
日期: 2024-12-04
說明: 從 Zip33U DBF 檔案匯入所有6碼郵遞區號資料到 SQL Server
      (含完整地址資料，約 80,000 筆)

使用方式:
    python 002_import_zipcode6_data.py --server localhost --database eservice_new --user sa --password YourPassword

    或使用環境變數:
    set SQL_SERVER=localhost
    set SQL_DATABASE=eservice_new
    set SQL_USER=sa
    set SQL_PASSWORD=YourPassword
    python 002_import_zipcode6_data.py

變更記錄:
    v1.1 - 匯入所有記錄 (非唯一), 新增 SCOOP 欄位
"""

import os
import sys
import argparse
from datetime import datetime

def main():
    parser = argparse.ArgumentParser(description='Import ZIPCODE6 data from DBF to SQL Server')
    parser.add_argument('--server', default=os.getenv('SQL_SERVER', 'localhost,1433'),
                        help='SQL Server host (default: localhost,1433)')
    parser.add_argument('--database', default=os.getenv('SQL_DATABASE', 'eservice_new'),
                        help='Database name (default: eservice_new)')
    parser.add_argument('--user', default=os.getenv('SQL_USER', 'sa'),
                        help='SQL Server user (default: sa)')
    parser.add_argument('--password', default=os.getenv('SQL_PASSWORD', ''),
                        help='SQL Server password')
    parser.add_argument('--dbf-path', default=None,
                        help='Path to rall1.dbf file')
    parser.add_argument('--dry-run', action='store_true',
                        help='Only extract data, do not insert to database')
    parser.add_argument('--batch-size', type=int, default=1000,
                        help='Batch size for insert operations (default: 1000)')
    parser.add_argument('--truncate', action='store_true',
                        help='Truncate existing data before import')

    args = parser.parse_args()

    print('=' * 60)
    print('ZIPCODE6 Data Import Script v1.1 (Full Data)')
    print(f'Started: {datetime.now().strftime("%Y-%m-%d %H:%M:%S")}')
    print('=' * 60)

    # 確認 DBF 路徑
    if args.dbf_path:
        dbf_path = args.dbf_path
    else:
        # 嘗試常見路徑
        possible_paths = [
            'documents/Zip33U/DBF/rall1.dbf',
            '../documents/Zip33U/DBF/rall1.dbf',
            '../../documents/Zip33U/DBF/rall1.dbf',
            'F:/AITest/MOH-CodeBaseNew/documents/Zip33U/DBF/rall1.dbf',
        ]
        dbf_path = None
        for path in possible_paths:
            if os.path.exists(path):
                dbf_path = path
                break

        if not dbf_path:
            print('✗ Error: Cannot find rall1.dbf file')
            print('  Please specify path using --dbf-path argument')
            sys.exit(1)

    print(f'DBF Path: {dbf_path}')

    # 安裝必要套件
    try:
        from dbfread import DBF
    except ImportError:
        print('Installing dbfread...')
        import subprocess
        subprocess.check_call([sys.executable, '-m', 'pip', 'install', 'dbfread', '-q'])
        from dbfread import DBF

    # ============================================
    # Step 1: 從 DBF 提取所有資料
    # ============================================
    print('\n[Step 1] Extracting ALL data from DBF file...')

    table = DBF(dbf_path, encoding='cp950', char_decode_errors='ignore')

    all_records = []
    total_records = 0
    skipped_records = 0

    for record in table:
        total_records += 1
        zipcode = record.get('ZIPCODE', '')

        # 只處理6碼郵遞區號
        if zipcode and len(str(zipcode)) == 6:
            zip3 = record.get('ZIP3A', '')
            if not zip3:
                zip3 = zipcode[:3]

            all_records.append({
                'ZIP_CO': zipcode,
                'ZIP3': zip3[:3],
                'CITYNM': record.get('CITY', '') or '',
                'TOWNNM': record.get('AREA', '') or '',
                'ROADNM': (record.get('ROAD', '') or '')[:100],
                'SCOOP': (record.get('SCOOP', '') or '')[:100]
            })
        else:
            skipped_records += 1

        if total_records % 20000 == 0:
            print(f'  Processed {total_records:,} records, collected {len(all_records):,}')

    print(f'✓ Total DBF records: {total_records:,}')
    print(f'✓ Valid 6-digit records: {len(all_records):,}')
    print(f'  Skipped (non 6-digit): {skipped_records:,}')

    # 統計唯一郵遞區號數量
    unique_codes = len(set(r['ZIP_CO'] for r in all_records))
    print(f'  Unique 6-digit codes: {unique_codes:,}')

    if args.dry_run:
        print('\n[Dry Run Mode] Skipping database insert')
        print('Sample data (first 10):')
        for i, data in enumerate(all_records[:10]):
            print(f'  {data["ZIP_CO"]}: {data["CITYNM"]}{data["TOWNNM"]} - {data["ROADNM"]}')
        print('=' * 60)
        print('Dry run completed')
        return

    # ============================================
    # Step 2: 連接 SQL Server
    # ============================================
    print('\n[Step 2] Connecting to SQL Server...')

    try:
        import pyodbc
    except ImportError:
        print('Installing pyodbc...')
        import subprocess
        subprocess.check_call([sys.executable, '-m', 'pip', 'install', 'pyodbc', '-q'])
        import pyodbc

    conn_str = (
        f"DRIVER={{ODBC Driver 17 for SQL Server}};"
        f"SERVER={args.server};"
        f"DATABASE={args.database};"
        f"UID={args.user};"
        f"PWD={args.password};"
        f"TrustServerCertificate=yes;"
    )

    try:
        conn = pyodbc.connect(conn_str)
        cursor = conn.cursor()
        print(f'✓ Connected to {args.server}/{args.database}')
    except Exception as e:
        print(f'✗ Connection failed: {e}')
        sys.exit(1)

    # ============================================
    # Step 3: 檢查表是否存在
    # ============================================
    print('\n[Step 3] Checking ZIPCODE6 table...')

    cursor.execute("SELECT COUNT(*) FROM sys.tables WHERE name = 'ZIPCODE6'")
    if cursor.fetchone()[0] == 0:
        print('✗ ZIPCODE6 table does not exist!')
        print('  Please run 001_create_zipcode6_table.sql first')
        conn.close()
        sys.exit(1)

    # 檢查現有資料
    cursor.execute("SELECT COUNT(*) FROM ZIPCODE6")
    existing_count = cursor.fetchone()[0]
    print(f'  Existing records in ZIPCODE6: {existing_count:,}')

    if existing_count > 0:
        if args.truncate:
            print('  Truncating existing data...')
            cursor.execute("TRUNCATE TABLE ZIPCODE6")
            conn.commit()
            print('  ✓ Table truncated')
        else:
            print('  ⚠ Table already has data. Use --truncate to clear first.')
            print('  Proceeding with INSERT (will add to existing data)...')

    # ============================================
    # Step 4: 匯入資料
    # ============================================
    print(f'\n[Step 4] Importing {len(all_records):,} records...')

    insert_sql = """
        INSERT INTO ZIPCODE6 (ZIP_CO, ZIP3, CITYNM, TOWNNM, ROADNM, SCOOP)
        VALUES (?, ?, ?, ?, ?, ?)
    """

    inserted = 0
    errors = 0
    batch_count = 0

    for data in all_records:
        try:
            cursor.execute(insert_sql, (
                data['ZIP_CO'],
                data['ZIP3'],
                data['CITYNM'],
                data['TOWNNM'],
                data['ROADNM'],
                data['SCOOP']
            ))
            inserted += 1
            batch_count += 1

            if batch_count >= args.batch_size:
                conn.commit()
                batch_count = 0
                print(f'  Progress: {inserted:,}/{len(all_records):,} ({inserted*100//len(all_records)}%)')

        except Exception as e:
            errors += 1
            if errors <= 5:
                print(f'  ✗ Error inserting record: {e}')

    # 提交剩餘的批次
    conn.commit()

    print(f'✓ Inserted: {inserted:,} records')
    if errors > 0:
        print(f'⚠ Errors: {errors}')

    # ============================================
    # Step 5: 驗證資料
    # ============================================
    print('\n[Step 5] Verifying data...')

    cursor.execute("SELECT COUNT(*) FROM ZIPCODE6")
    final_count = cursor.fetchone()[0]
    print(f'  Total records in ZIPCODE6: {final_count:,}')

    cursor.execute("SELECT COUNT(DISTINCT ZIP_CO) FROM ZIPCODE6")
    unique_count = cursor.fetchone()[0]
    print(f'  Unique 6-digit codes: {unique_count:,}')

    # 驗證特定郵遞區號 (衛福部地址)
    cursor.execute("""
        SELECT TOP 3 ZIP_CO, CITYNM, TOWNNM, ROADNM
        FROM ZIPCODE6
        WHERE ZIP_CO = '115204'
    """)
    rows = cursor.fetchall()
    if rows:
        print(f'  ✓ Verification: 115204 found ({len(rows)} records)')
        for row in rows:
            print(f'      {row[0]}: {row[1]}{row[2]} - {row[3]}')
    else:
        print(f'  ⚠ Verification: 115204 not found')

    # 記錄 Migration
    cursor.execute("""
        IF NOT EXISTS (SELECT 1 FROM DB_MIGRATION_LOG WHERE MIGRATION_NAME = 'ZIPCODE6_DATA_V1.1')
        BEGIN
            INSERT INTO DB_MIGRATION_LOG (MIGRATION_NAME, DESCRIPTION)
            VALUES ('ZIPCODE6_DATA_V1.1', N'匯入6碼郵遞區號完整資料 ' + CAST(? AS NVARCHAR) + N' 筆')
        END
    """, (final_count,))
    conn.commit()

    cursor.close()
    conn.close()

    print('\n' + '=' * 60)
    print('✓ Import completed successfully')
    print(f'  Total records: {final_count:,}')
    print(f'  Unique codes: {unique_count:,}')
    print(f'Finished: {datetime.now().strftime("%Y-%m-%d %H:%M:%S")}')
    print('=' * 60)


if __name__ == '__main__':
    main()
