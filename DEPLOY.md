# Деплой на Render (тестовый)

1. Закоммить и запушить изменения в `main`:
   `git add . && git commit -m "Add Render deploy" && git push`
2. На https://dashboard.render.com: **New → Blueprint** → выбрать репозиторий `behaatel`. Render прочитает `render.yaml` и создаст базу, бэкенд и фронтенд.
3. Render попросит значения (секреты):
   - `SeedAdmin__Password` — пароль первого супер-админа (email: `superadmin@behaatel.com`)
   - `Email__User`, `Email__FromAddress` — Gmail-адрес
   - `Email__Password` — пароль приложения Gmail (не обычный пароль)
   - `Cors__AllowedOrigins__0` и `VITE_API_URL` — пока любые, исправить на шаге 5
4. Дождаться сборки. Получатся два адреса:
   - бэкенд: `https://behaatel-api.onrender.com`
   - фронтенд: `https://behaatel-web.onrender.com`
   (если имена заняты, Render добавит суффикс — смотри точные адреса в панели)
5. Поправить переменные:
   - `behaatel-api` → `Cors__AllowedOrigins__0` = адрес фронтенда (без `/` в конце)
   - `behaatel-web` → `VITE_API_URL` = `<адрес бэкенда>/api/v1` и нажать **Manual Deploy**
6. Открыть адрес фронтенда и войти как `superadmin@behaatel.com`.

Заметки: на бесплатном плане сервис засыпает после ~15 минут без запросов, первый запрос после паузы идёт около минуты. Бесплатная база Render живёт 30 дней.
