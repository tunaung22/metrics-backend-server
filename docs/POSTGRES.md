# PostgreSQL

## Instructions

### Add Postgres to environment path

> C:\Program Files\PostgreSQL\17\bin

1. Go to System Settings > About > Advanced System Settings
2. Advanced Tab > Environment Variables
3. System Variables > New (add the following)
    - Variable name: ```POSTGRES```
    - Variable value: ```C:\Program Files\PostgreSQL\17```
4. System Variables > Path > Edit > New
    - ```%POSTGRES%\bin```
