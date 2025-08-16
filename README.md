# Kitchen After Sales - Enhanced

- Backend: ASP.NET Core 8, JWT + Roles, DTO + FluentValidation, Soft delete + Audit log, Paging/Sorting/Filtering
- Frontend: Angular 18, Syncfusion Grid (server-side), Scheduler, role-based UI
- Deploy: Docker multi-stage, Nginx reverse proxy

## Run locally (Docker)

```bash
docker build -t kitchen-after-sales:latest .
docker run -p 8080:80 -v $(pwd)/backend/uploads:/app/uploads kitchen-after-sales:latest
```

Then open http://localhost:8080

- API base: `/api`
- Uploads static: `/uploads/*` (volume mounted)

## Roles
- Default `AppUser.Role` values: `Admin`, `Dispatcher`, `Technician`, `User`
- Controllers restrict writes by policies; UI hides buttons by `AuthService.hasRole`

## Notes
- Syncfusion Grid uses server-side data via UrlAdaptor; backend responds with `{ result, count }`
- Scheduler fetches events with `start`/`end` range