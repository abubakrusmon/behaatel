#!/usr/bin/env bash
set -u

API_BASE_URL="${API_BASE_URL:-http://localhost:5000}"
: "${SUPERADMIN_EMAIL:?Set SUPERADMIN_EMAIL}"
: "${SUPERADMIN_PASSWORD:?Set SUPERADMIN_PASSWORD}"
: "${SMOKE_ADMIN_EMAIL:?Set SMOKE_ADMIN_EMAIL}"
: "${SMOKE_ADMIN_PASSWORD:?Set SMOKE_ADMIN_PASSWORD}"
: "${SMOKE_RESIDENT_EMAIL:?Set SMOKE_RESIDENT_EMAIL}"
: "${SMOKE_RESIDENT_PASSWORD:?Set SMOKE_RESIDENT_PASSWORD}"

RUN_TAG="${SMOKE_RUN_TAG:-$(date -u +%Y%m%d%H%M%S)-$RANDOM}"

RESULTS=()
RESP_BODY=""
RESP_STATUS="000"
REPORT_PRINTED=0
CSV_FILE=""

print_table() {
    if [[ "$REPORT_PRINTED" == "1" ]]; then
        return
    fi
    REPORT_PRINTED=1
    printf '\n%-45s | %-16s | %-8s | %s\n' "Сценарий" "Ожидался" "Факт" "Результат"
    printf '%s\n' '----------------------------------------------+------------------+----------+----------'
    for row in "${RESULTS[@]}"; do
        IFS='|' read -r scenario expected actual result <<< "$row"
        printf '%-45s | %-16s | %-8s | %s\n' "$scenario" "$expected" "$actual" "$result"
    done
}
cleanup() {
    if [[ -n "$CSV_FILE" ]]; then
        rm -f "$CSV_FILE"
    fi
    print_table
}
trap cleanup EXIT

record_result() {
    local scenario="$1"
    local expected="$2"
    local actual="$3"
    local result="PASS"
    if [[ "$expected" != "$actual" ]]; then
        result="FAIL"
    fi
    RESULTS+=("$scenario|$expected|$actual|$result")
    printf '%-45s expected=%-8s actual=%-8s %s\n' "$scenario" "$expected" "$actual" "$result"
}

record_contract() {
    local scenario="$1"
    local status="$2"
    local passed="$3"
    local result="PASS"
    if [[ "$passed" != "true" ]]; then
        result="FAIL"
    fi
    RESULTS+=("$scenario|HTTP $status + contract|HTTP $status; assertion=$passed|$result")
    printf '%-45s HTTP=%-3s contract=%-5s %s\n' "$scenario" "$status" "$passed" "$result"
}

short_body() {
    printf '%s' "$1" |
        sed -E 's/("(accessToken|refreshToken|passwordHash)"[[:space:]]*:[[:space:]]*)"[^"]*"/\1"[REDACTED]"/Ig' |
        tr '\n' ' ' |
        cut -c1-220
}

request() {
    local method="$1"
    local url="$2"
    local token="${3:-}"
    local body="${4:-}"
    local output
    local args=(-sS --max-time 20 -X "$method" -H "Accept: application/json")
    if [[ -n "$token" ]]; then
        args+=(-H "Authorization: Bearer $token")
    fi
    if [[ -n "$body" ]]; then
        args+=(-H "Content-Type: application/json" --data "$body")
    fi

    output="$(curl "${args[@]}" -w $'\n%{http_code}' "$url")" || {
        RESP_BODY="curl request failed"
        RESP_STATUS="000"
        printf 'HTTP %s %s\n' "$RESP_STATUS" "$(short_body "$RESP_BODY")"
        return
    }
    RESP_STATUS="${output##*$'\n'}"
    RESP_BODY="${output%$'\n'*}"
    printf 'HTTP %s %s\n' "$RESP_STATUS" "$(short_body "$RESP_BODY")"
}

json_field() {
    local match
    match="$(printf '%s' "$1" |
        grep -oE "\"$2\"[[:space:]]*:[[:space:]]*(\"[^\"]*\"|[^,}]+)" |
        sed -n '1p')" || true
    printf '%s' "$match" |
        sed -E 's/^[^:]+:[[:space:]]*"?([^"]*)"?$/\1/'
}

json_escape() {
    local value="$1"
    value="${value//\\/\\\\}"
    value="${value//\"/\\\"}"
    value="${value//$'\n'/\\n}"
    value="${value//$'\r'/\\r}"
    value="${value//$'\t'/\\t}"
    printf '%s' "$value"
}

require_status() {
    local scenario="$1"
    local expected="$2"
    record_result "$scenario" "$expected" "$RESP_STATUS"
    if [[ "$RESP_STATUS" != "$expected" ]]; then
        print_table
        exit 1
    fi
}

add_run_tag() {
    local email="$1"
    local local_part="${email%@*}"
    local domain="${email#*@}"
    printf '%s+%s@%s' "$local_part" "$RUN_TAG" "$domain"
}

api="$API_BASE_URL"
admin_email="$(add_run_tag "$SMOKE_ADMIN_EMAIL")"
resident_email="$(add_run_tag "$SMOKE_RESIDENT_EMAIL")"

request GET "$api/health"
require_status "1. Health check" 200

request POST "$api/api/v1/auth/login" "" \
    "{\"email\":\"$SUPERADMIN_EMAIL\",\"password\":\"$SUPERADMIN_PASSWORD\"}"
require_status "2. SuperAdmin login" 200
superadmin_token="$(json_field "$RESP_BODY" accessToken)"
if [[ -z "$superadmin_token" ]]; then
    record_contract "2. Access token returned" "$RESP_STATUS" false
    print_table
    exit 1
fi

request GET "$api/api/v1/auth/me" "$superadmin_token"
require_status "3. Current SuperAdmin profile" 200
profile_ok=false
if printf '%s' "$RESP_BODY" | grep -q '"mustChangePassword"[[:space:]]*:[[:space:]]*true' &&
   printf '%s' "$RESP_BODY" | grep -qi '"role"[[:space:]]*:[[:space:]]*"superAdmin"'; then
    profile_ok=true
fi
record_contract "3. mustChangePassword and role claims" "$RESP_STATUS" "$profile_ok"

admin_email_json="$(json_escape "$admin_email")"
admin_password_json="$(json_escape "$SMOKE_ADMIN_PASSWORD")"
admin_body="{\"email\":\"$admin_email_json\",\"fullName\":\"Smoke Admin\",\"phone\":null,\"password\":\"$admin_password_json\",\"permissions\":[\"residentsView\",\"residentsManage\",\"roomsView\",\"roomsManage\"]}"
request POST "$api/api/v1/superadmin/admins" "$superadmin_token" "$admin_body"
require_status "4a. Create admin" 201
admin_id="$(json_field "$RESP_BODY" id)"
request POST "$api/api/v1/superadmin/admins" "$superadmin_token" "$admin_body"
require_status "4b. Duplicate admin email" 409

request POST "$api/api/v1/admin/buildings" "$superadmin_token" \
    "{\"name\":\"Smoke Building $RUN_TAG\",\"address\":\"Smoke Test Address\",\"floorsCount\":4}"
require_status "5a. Create building" 201
building_id="$(json_field "$RESP_BODY" id)"
if [[ -z "$building_id" ]]; then
    record_contract "5a. Building ID returned" "$RESP_STATUS" false
    print_table
    exit 1
fi

request POST "$api/api/v1/admin/rooms" "$superadmin_token" \
    "{\"buildingId\":\"$building_id\",\"number\":\"SM-$RUN_TAG\",\"floor\":1,\"capacity\":2,\"pricePerBed\":300,\"type\":\"standard\"}"
require_status "5b. Create room" 201
room_id="$(json_field "$RESP_BODY" id)"
if [[ -z "$room_id" ]]; then
    record_contract "5b. Room ID returned" "$RESP_STATUS" false
    print_table
    exit 1
fi

resident_email_json="$(json_escape "$resident_email")"
resident_password_json="$(json_escape "$SMOKE_RESIDENT_PASSWORD")"
resident_body="{\"email\":\"$resident_email_json\",\"fullName\":\"Smoke Resident\",\"phone\":\"+992900000000\",\"password\":\"$resident_password_json\",\"passportNumber\":\"SMOKE-$RUN_TAG\",\"birthDate\":\"2000-01-01\",\"organization\":\"Smoke Test\",\"roomId\":\"$room_id\",\"checkInDate\":\"$(date -u +%Y-%m)-01\"}"
request POST "$api/api/v1/admin/residents" "$superadmin_token" "$resident_body"
require_status "6a. Create and check in resident" 201
resident_id="$(json_field "$RESP_BODY" id)"
if [[ -z "$resident_id" ]]; then
    record_contract "6a. Resident ID returned" "$RESP_STATUS" false
    print_table
    exit 1
fi
request POST "$api/api/v1/admin/residents" "$superadmin_token" "$resident_body"
require_status "6b. Duplicate resident email" 409

current_period="$(date -u +%Y-%m)"
request POST "$api/api/v1/admin/charges/generate?period=$current_period" "$superadmin_token"
require_status "7a. Generate monthly charge" 200
request POST "$api/api/v1/admin/charges/generate?period=$current_period" "$superadmin_token"
require_status "7b. Idempotent generation" 200
generation_ok=false
if [[ "$(json_field "$RESP_BODY" created)" == "0" ]]; then
    generation_ok=true
fi
record_contract "7b. Second run creates zero charges" "$RESP_STATUS" "$generation_ok"

request POST "$api/api/v1/admin/payments" "$superadmin_token" \
    "{\"residentId\":\"$resident_id\",\"amount\":100,\"method\":\"cash\"}"
require_status "8a. Create partial payment" 200
request GET "$api/api/v1/admin/payments?page=1&pageSize=20&search=Smoke%20Resident" "$superadmin_token"
require_status "8b. List payments" 200
request GET "$api/api/v1/admin/debtors?page=1&pageSize=20&search=Smoke%20Resident" "$superadmin_token"
require_status "8c. Debtor appears in debtors" 200
debt_ok=false
if printf '%s' "$RESP_BODY" | grep -q "$resident_id"; then debt_ok=true; fi
record_contract "8c. Resident appears in debtor results" "$RESP_STATUS" "$debt_ok"
request GET "$api/api/v1/admin/dashboard" "$superadmin_token"
require_status "8d. Dashboard summary" 200

request POST "$api/api/v1/auth/login" "" \
    "{\"email\":\"$resident_email\",\"password\":\"$SMOKE_RESIDENT_PASSWORD\"}"
require_status "9a. Resident login" 200
resident_token="$(json_field "$RESP_BODY" accessToken)"
if [[ -z "$resident_token" ]]; then
    record_contract "9a. Resident access token returned" "$RESP_STATUS" false
    print_table
    exit 1
fi
request GET "$api/api/v1/me/summary" "$resident_token"
require_status "9b. Resident financial summary" 200
summary_ok=false
if printf '%s' "$RESP_BODY" | grep -q '"balance"[[:space:]]*:[[:space:]]*-200' &&
   printf '%s' "$RESP_BODY" | grep -q '"dueThisMonth"[[:space:]]*:[[:space:]]*200'; then
    summary_ok=true
fi
record_contract "9b. Balance=-200 and DueThisMonth=200" "$RESP_STATUS" "$summary_ok"
request GET "$api/api/v1/me/charts/monthly?months=12" "$resident_token"
require_status "9c. Resident monthly chart" 200

request GET "$api/api/v1/admin/dashboard" "$resident_token"
require_status "10a. Resident forbidden from admin area" 403
limited_admin_token=""
if [[ -n "$admin_id" ]]; then
    request POST "$api/api/v1/auth/login" "" \
        "{\"email\":\"$admin_email\",\"password\":\"$SMOKE_ADMIN_PASSWORD\"}"
    if [[ "$RESP_STATUS" == "200" ]]; then
        limited_admin_token="$(json_field "$RESP_BODY" accessToken)"
    fi
fi
if [[ -n "$limited_admin_token" ]]; then
    request POST "$api/api/v1/admin/payments" "$limited_admin_token" \
        "{\"residentId\":\"$resident_id\",\"amount\":50,\"method\":\"cash\"}"
    require_status "10b. Admin without PaymentsManage forbidden" 403
else
    record_result "10b. Admin without PaymentsManage forbidden" 403 "NO_ADMIN_TOKEN"
fi
request GET "$api/api/v1/admin/dashboard"
require_status "10c. Protected endpoint without token" 401

CSV_FILE="$(mktemp)"
csv_status="$(curl -sS --max-time 20 -o "$CSV_FILE" -w '%{http_code}' \
    -H "Authorization: Bearer $superadmin_token" \
    "$api/api/v1/admin/payments/export")" || csv_status="000"
printf 'HTTP %s CSV export\n' "$csv_status"
record_result "11. Payments CSV export" 200 "$csv_status"
csv_contract=false
if [[ "$csv_status" == "200" ]] &&
   [[ "$(od -An -tx1 -N3 "$CSV_FILE" | tr -d ' \n')" == "efbbbf" ]] &&
   grep -q ';' "$CSV_FILE"; then
    csv_contract=true
fi
record_contract "11. CSV has UTF-8 BOM and semicolon delimiter" "$csv_status" "$csv_contract"

for row in "${RESULTS[@]}"; do
    if [[ "$row" == *"|FAIL" ]]; then
        exit 1
    fi
done
