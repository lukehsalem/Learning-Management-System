import os
import re
import requests
import yaml
from typing import Any, Dict, List, Optional

API = "https://api.github.com"
ID_RE = re.compile(r"<!--\s*backlog-id:\s*([A-Za-z0-9._-]+)\s*-->")

def gh_headers(token: str) -> Dict[str, str]:
    return {
        "Authorization": f"Bearer {token}",
        "Accept": "application/vnd.github+json",
        "X-GitHub-Api-Version": "2022-11-28",
    }

def paginate(url: str, token: str, params: Dict[str, Any]) -> List[Any]:
    out: List[Any] = []
    page = 1
    while True:
        p = dict(params)
        p["per_page"] = 100
        p["page"] = page
        r = requests.get(url, headers=gh_headers(token), params=p)
        r.raise_for_status()
        chunk = r.json()
        if not chunk:
            break
        out.extend(chunk)
        if len(chunk) < 100:
            break
        page += 1
    return out

def strip_marker(body: str) -> str:
    # remove marker line only; keep rest as-is
    return ID_RE.sub("", body).lstrip()

def main() -> None:
    token = os.environ["GITHUB_TOKEN"]
    backlog_label = os.environ.get("BACKLOG_LABEL", "course-backlog")
    output_file = os.environ.get("OUTPUT_FILE", "backlog/issues.yml")
    strip = os.environ.get("STRIP_ID_MARKER_FROM_BODY", "true").lower() == "true"

    owner, repo = os.environ["GITHUB_REPOSITORY"].split("/")

    # Fetch issues in backlog
    issues = paginate(
        f"{API}/repos/{owner}/{repo}/issues",
        token,
        params={"state": "open", "labels": backlog_label},
    )
    issues = [i for i in issues if "pull_request" not in i]

    # Collect milestones referenced
    milestone_titles = sorted({(i.get("milestone") or {}).get("title") for i in issues if i.get("milestone")})
    milestones_all = paginate(
        f"{API}/repos/{owner}/{repo}/milestones",
        token,
        params={"state": "all"},
    )
    milestones_map = {m["title"]: m for m in milestones_all}
    milestones_out = []
    for t in milestone_titles:
        m = milestones_map.get(t)
        if not m:
            continue
        milestones_out.append({
            "title": m["title"],
            "description": m.get("description") or "",
            "state": m.get("state") or "open",
        })

    # Fetch labels (optional) and export only those used by the backlog issues
    used_labels = set()
    for i in issues:
        for l in i.get("labels") or []:
            used_labels.add(l["name"])

    labels_all = paginate(f"{API}/repos/{owner}/{repo}/labels", token, params={})
    labels_out = []
    for l in labels_all:
        if l["name"] in used_labels:
            labels_out.append({
                "name": l["name"],
                "color": l.get("color") or "ededed",
                "description": l.get("description") or "",
            })
    labels_out.sort(key=lambda x: x["name"].lower())

    # Convert issues
    issues_out = []
    for i in issues:
        body = i.get("body") or ""
        m = ID_RE.search(body)
        backlog_id = m.group(1) if m else f"ISSUE-{i['number']}"  # fallback; ideally Ensure-IDs avoids this
        body_out = strip_marker(body) if strip else body

        issue_labels = sorted([l["name"] for l in (i.get("labels") or [])])
        milestone_title = (i.get("milestone") or {}).get("title")

        issues_out.append({
            "id": backlog_id,
            "title": i["title"],
            "body": body_out,
            "labels": issue_labels,
            **({"milestone": milestone_title} if milestone_title else {}),
        })

    # Stable ordering: by milestone then id
    def sort_key(x: Dict[str, Any]):
        return ((x.get("milestone") or ""), x["id"])
    issues_out.sort(key=sort_key)

    doc = {
        "version": 1,
        "labels": labels_out,
        "milestones": milestones_out,
        "issues": issues_out,
    }

    os.makedirs(os.path.dirname(output_file), exist_ok=True)
    with open(output_file, "w", encoding="utf-8") as f:
        yaml.safe_dump(doc, f, sort_keys=False, allow_unicode=True)

    print(f"Wrote {output_file} ({len(issues_out)} issues)")

if __name__ == "__main__":
    main()
