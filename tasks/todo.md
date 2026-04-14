# QA Studio - Build Tasks

## Step 6: Next.js Web App Scaffold ✅ (merged)
- React Query + Auth context
- Login, Register, Protected layout with sidebar
- Dashboard, Environments, Test Cases, Test Runs pages
- Full TypeScript, light/dark mode tokens

## Step 7: Playwright Recording (IN PROGRESS)

### Goal
QA engineers open a browser extension, navigate to Bento Studio (staging/prod), interact naturally, and watch steps appear in the QA Studio UI in real-time. When done, save as a test case.

### Architecture

```
[Chrome Extension]  →  [Backend API (SSE)]  →  [Frontend Recording Page]
  (content script)      /api/recording/         (live step preview)
```

### Backend - Recording Session Entity
- `RecordingSession`: Id, UserId, TargetUrl, StepsJson ("[]"), Status (Active/Completed/Cancelled), CreatedAt
- Steps stored as JSON array: `[{ action, selector?, value?, description? }]`

### Backend - API Endpoints
| Method | Path | Description |
|--------|------|-------------|
| POST | /api/recording/start | Create a new session, returns sessionId |
| GET | /api/recording/{id}/events | SSE stream — new steps pushed as JSON lines |
| POST | /api/recording/{id}/steps | Add a step to the session (called by extension) |
| DELETE | /api/recording/{id} | Cancel/delete a session |
| POST | /api/recording/{id}/save | Save session as a test case |

### Browser Extension
- `manifest.json` — Chrome extension manifest v3
- `popup/popup.html + popup.ts` — Start/stop recording, enter session ID
- `content-script.ts` — Intercepts DOM events (click, input, navigation), calls backend
- `background.ts` — Handles extension ↔ backend communication

### Frontend
- `/record/page.tsx` — Recording studio: start session, live step list via SSE, save as test case
- `src/services/recording.ts` — API calls to recording endpoints
- Sidebar nav: "Record" link with recording icon

### Files to create

**Backend:**
- [ ] `Domain/Entities/RecordingSession.cs`
- [ ] `Domain/Interfaces/IRecordingSessionRepository.cs`
- [ ] `Application/Recording/DTOs/` — CreateSessionDto, RecordingSessionDto, RecordingStepDto
- [ ] `Application/Recording/Interfaces/IRecordingService.cs`
- [ ] `Infrastructure/Repositories/RecordingSessionRepository.cs`
- [ ] `Infrastructure/Services/RecordingService.cs`
- [ ] `Infrastructure/Mappings/RecordingMappingProfile.cs`
- [ ] `Api/Controllers/RecordingController.cs`
- [ ] Register in `ServiceExtensions.cs`

**Browser Extension:**
- [ ] `extensions/recorder/manifest.json`
- [ ] `extensions/recorder/popup/popup.html`
- [ ] `extensions/recorder/popup/popup.ts`
- [ ] `extensions/recorder/content-script.ts`
- [ ] `extensions/recorder/background.ts`

**Frontend:**
- [ ] `src/services/recording.ts`
- [ ] `src/app/(protected)/record/page.tsx`
- [ ] Update sidebar to include "Record" nav item

### Step-by-step implementation order
1. Backend entity + repository
2. Backend DTOs + interface
3. RecordingService
4. RecordingController with SSE endpoint
5. AutoMapper profile + DI registration
6. `pnpm lint` + `pnpm typecheck`
7. Push PR → merge
8. Browser extension files
9. Frontend recording page + service
10. Push PR → merge

---

## Step 8: Playwright Execution
## Step 9: Reports Dashboard
## Step 10: Scheduling + Notifications
## Step 11: Production Deploy
