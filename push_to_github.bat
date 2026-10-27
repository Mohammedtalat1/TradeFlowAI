@echo off
echo ==============================================
echo  TradeFlowAI - Pushing to GitHub Repository
echo ==============================================

git init
git add .
git commit -m "feat: complete TradeFlowAI platform with Clean Architecture, CQRS, and 12 Design Patterns"
git branch -M main
git remote remove origin 2>nul
git remote add origin https://github.com/Mohammedtalat1/TradeFlowAI.git
git push -u origin main

echo.
echo ==============================================
echo  Done! Visit: https://github.com/Mohammedtalat1/TradeFlowAI
echo ==============================================
pause
