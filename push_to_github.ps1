Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "  TradeFlowAI - Pushing to GitHub Repository" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan

git init
git add .
git commit -m "feat: complete TradeFlowAI platform with Clean Architecture, CQRS, and 12 Design Patterns"
git branch -M main
git remote remove origin 2>$null
git remote add origin https://github.com/Mohammedtalat1/TradeFlowAI.git
git push -u origin main

Write-Host "`n==============================================" -ForegroundColor Green
Write-Host "  Done! Visit: https://github.com/Mohammedtalat1/TradeFlowAI" -ForegroundColor Green
Write-Host "==============================================" -ForegroundColor Green
