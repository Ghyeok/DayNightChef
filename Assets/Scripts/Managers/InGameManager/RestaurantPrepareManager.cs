using System;
using System.Collections.Generic;
using UnityEngine;

public class RestaurantPrepareManager : SingletonManager<RestaurantPrepareManager>
{
    [SerializeField] private List<MenuPlan> todayMenu = new List<MenuPlan>();

    // 등록할 수 있는 레시피의 개수, 레스토랑 레벨에 따라 달라짐
    [SerializeField] private int maxTotalItemCount = 7;

    public List<MenuPlan> TodayMenu => todayMenu;

    /// <summary>
    /// 현재 등록한 모든 레시피의 총 개수
    /// </summary>
    public int CurTotalPlannedCount
    {
        get
        {
            int total = 0;
            foreach (var plan in todayMenu)
            {
                total += plan.planned;
            }
            return total;
        }
    }

    /// <summary>
    /// 메뉴에 추가할 수 있는 남은 레시피 개수
    /// </summary>
    public int RemainingItemCapacity => maxTotalItemCount - CurTotalPlannedCount;

    /// <summary>
    /// 최대 등록 가능한 레시피 개수 (UI 표시용)
    /// </summary>
    public int MaxTotalItemCount => maxTotalItemCount;

    public event Action OnMenuChanged;

    void Start()
    {

    }

    /// <summary>
    /// 메뉴 플랜을 즉시 변경하고 재료를 차감/환불합니다.
    /// 성공 시 true, 재료/한도 부족 시 false를 반환합니다.
    /// </summary>
    public bool UpdateMenuPlan(Recipe recipe, int newQuantity)
    {
        if (recipe == null) return false;

        MenuPlan existingPlan = todayMenu.Find(p => p.recipe == recipe);
        int oldQuantity = (existingPlan != null) ? existingPlan.planned : 0;

        // 1. 수량 변화량(delta) 계산
        int delta = newQuantity - oldQuantity;

        if (delta == 0) return true; // 변경 사항 없음

        // 2. 총 개수 한도 검사 (차감/환불 '전에' 수행)
        int totalWithoutThis = CurTotalPlannedCount - oldQuantity; // 현재 레시피를 추가하기 전의 등록된 요리 개수
        int projectedTotal = totalWithoutThis + newQuantity; // 현재 레시피를 추가한 후의 등록된 요리 개수

        if (projectedTotal > maxTotalItemCount)
        {
            Debug.LogWarning($"총 메뉴 개수 한도({maxTotalItemCount}개)를 초과합니다!");
            return false; // 총 한도 초과
        }

        // 3. 재료 차감 (delta > 0)
        if (delta > 0)
        {
            int amountToConsume = delta;

            // 창고에 'delta'만큼의 재료가 있는지 확인하고 '차감'
            bool consumeSuccess = WarehouseManager.Instance.TryConsumeForRecipe(recipe, amountToConsume);

            if (!consumeSuccess)
            {
                Debug.LogWarning($"[재료 부족] {recipe.recipe_name} {amountToConsume}개 분량의 재료가 부족합니다.");
                return false; // 재료 부족
            }

            Debug.Log($"[재료 차감] {recipe.recipe_name} {amountToConsume}개분 차감됨.");
        }
        // 4. 재료 환불 (delta < 0)
        else if (delta < 0)
        {
            int amountToRefund = -delta; 

            foreach (var need in recipe.recipe_requireItems)
            {
                WarehouseManager.Instance.TryAdd(need.item, need.count * amountToRefund);
            }
            Debug.Log($"[재료 환불] {recipe.recipe_name} {amountToRefund}개분 환불됨.");
        }

        // 5. 모든 검사 통과 -> 리스트(todayMenu) 갱신
        if (existingPlan != null)
        {
            if (newQuantity > 0)
                existingPlan.planned = newQuantity; // 수량 변경
            else
                todayMenu.Remove(existingPlan); // 0개는 제거
        }
        else if (newQuantity > 0)
        {
            todayMenu.Add(new MenuPlan { recipe = recipe, planned = newQuantity, sold = 0 }); // 새로 추가
        }

        OnMenuChanged?.Invoke(); // UI 갱신 이벤트 호출
        return true; // 성공
    }

    /// <summary>
    /// 특정 레시피의 현재 계획된 수량을 반환합니다.
    /// </summary>
    public int GetPlannedCount(Recipe recipe)
    {
        if (recipe == null) return 0;
        MenuPlan existingPlan = todayMenu.Find(p => p.recipe == recipe);
        return (existingPlan != null) ? existingPlan.planned : 0;
    }

    /// <summary>
    /// 모든 메뉴 계획을 초기화합니다.
    /// </summary>
    public void ClearPlan()
    {
        if (todayMenu.Count > 0)
        {
            todayMenu.Clear();
            Debug.Log("[Menu] 모든 계획 초기화");
            OnMenuChanged?.Invoke();
        }
    }
}