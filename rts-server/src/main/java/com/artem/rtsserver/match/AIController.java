package com.artem.rtsserver.match;

public class AIController {

    public enum Difficulty {
        EASY, NORMAL, HARD
    }

    private enum AIState {
        SCOUT, ECONOMY, BUILD_ARMY, ATTACK, DEFEND, RETREAT
    }

    private final int aiPlayerId;
    private AIState state = AIState.SCOUT;

    private float thinkTimer = 0f;
    private float scoutTimer = 0f;
    private float economyTimer = 0f;
    private float trainTimer = 0f;
    private float buildTimer = 0f;

    private float visionRange;
    private int minAttackArmy;
    private int maxArmy;
    private int targetWorkers;
    private float thinkInterval;
    private float trainInterval;

    private Integer knownEnemyBuildingId = null;
    private Integer knownEnemyUnitId = null;

    public AIController(int aiPlayerId, Difficulty difficulty) {
        this.aiPlayerId = aiPlayerId;

        switch (difficulty) {
            case EASY -> {
                visionRange = 4f;
                minAttackArmy = 5;
                maxArmy = 5;
                targetWorkers = 2;
                thinkInterval = 2.5f;
                trainInterval = 8f;
            }
            case NORMAL -> {
                visionRange = 6f;
                minAttackArmy = 4;
                maxArmy = 8;
                targetWorkers = 3;
                thinkInterval = 1.5f;
                trainInterval = 5f;
            }
            case HARD -> {
                visionRange = 8f;
                minAttackArmy = 3;
                maxArmy = 12;
                targetWorkers = 5;
                thinkInterval = 0.8f;
                trainInterval = 3f;
            }
        }

        System.out.println("[AI] Difficulty = " + difficulty);
    }

    public void update(GameSession session, float dt) {
        thinkTimer += dt;
        scoutTimer += dt;
        economyTimer += dt;
        trainTimer += dt;
        buildTimer += dt;

        if (thinkTimer < thinkInterval)
            return;

        thinkTimer = 0f;

        PlayerState ai = session.getPlayersState().get(aiPlayerId);
        if (ai == null)
            return;

        checkVision(session);

        if (hasEnemyNearBase(session)) {
            state = AIState.DEFEND;
        } else if (hasWeakArmy(session)) {
            state = AIState.RETREAT;
        }

        switch (state) {
            case SCOUT -> updateScout(session);
            case ECONOMY -> updateEconomy(session, ai);
            case BUILD_ARMY -> updateBuildArmy(session, ai);
            case ATTACK -> updateAttack(session);
            case DEFEND -> updateDefend(session);
            case RETREAT -> updateRetreat(session);
        }
    }

    private void updateScout(GameSession session) {
        if (knownEnemyBuildingId != null || knownEnemyUnitId != null) {
            state = AIState.ECONOMY;
            System.out.println("[AI] Enemy discovered -> ECONOMY");
            return;
        }

        if (scoutTimer < 4f)
            return;

        scoutTimer = 0f;

        UnitState scout = findAnyCombatUnit(session);
        if (scout == null)
            scout = findAnyWorker(session);

        if (scout == null)
            return;

        scout.setTarget(0f, 0f);
        System.out.println("[AI] Scouting with unit " + scout.getId());
    }

    private void updateEconomy(GameSession session, PlayerState ai) {
        if (economyTimer >= 2f) {
            economyTimer = 0f;

            sendWorkersToResources(session);
            rebuildEconomy(session, ai);

            if (countWorkers(session) < targetWorkers) {
                trainWorker(session, ai);
                return;
            }

            if (ai.getMaxSupply() - ai.getUsedSupply() <= 2) {
                buildHouse(session, ai);
                return;
            }
        }

        state = AIState.BUILD_ARMY;
    }

    private void updateBuildArmy(GameSession session, PlayerState ai) {
        rebuildMilitary(session, ai);

        int army = countCombatUnits(session);

        if (army >= minAttackArmy) {
            state = AIState.ATTACK;
            System.out.println("[AI] Army ready -> ATTACK");
            return;
        }

        if (trainTimer < trainInterval)
            return;

        trainTimer = 0f;

        if (army < maxArmy)
            trainCombatUnits(session, ai);
    }

    private void updateAttack(GameSession session) {
        chooseBestVisibleTarget(session);

        if (knownEnemyBuildingId == null && knownEnemyUnitId == null) {
            state = AIState.SCOUT;
            System.out.println("[AI] No target -> SCOUT");
            return;
        }

        for (UnitState u : session.getUnits().values()) {
            if (u.getOwnerPlayerId() != aiPlayerId)
                continue;

            if ("worker".equals(u.getUnitType()))
                continue;

            if (knownEnemyUnitId != null) {
                u.setAttackTarget(knownEnemyUnitId);
                u.setAttackTargetIsBuilding(false);
            } else {
                u.setAttackTarget(knownEnemyBuildingId);
                u.setAttackTargetIsBuilding(true);
            }
        }
    }

    private void updateDefend(GameSession session) {
        UnitState enemy = findEnemyNearBase(session);

        if (enemy == null) {
            state = AIState.BUILD_ARMY;
            System.out.println("[AI] Base safe -> BUILD_ARMY");
            return;
        }

        for (UnitState u : session.getUnits().values()) {
            if (u.getOwnerPlayerId() != aiPlayerId)
                continue;

            if ("worker".equals(u.getUnitType()))
                continue;

            u.setAttackTarget(enemy.getId());
            u.setAttackTargetIsBuilding(false);
        }

        System.out.println("[AI] Defending base");
    }

    private void updateRetreat(GameSession session) {
        BuildingState base = findOwnCastle(session);
        if (base == null) {
            state = AIState.ECONOMY;
            return;
        }

        for (UnitState u : session.getUnits().values()) {
            if (u.getOwnerPlayerId() != aiPlayerId)
                continue;

            if ("worker".equals(u.getUnitType()))
                continue;

            if (u.getHp() < u.getMaxHp() * 0.35f) {
                u.setTarget(base.getX(), base.getY());
            }
        }

        state = AIState.ECONOMY;
        System.out.println("[AI] Retreat weak units");
    }

    private void rebuildEconomy(GameSession session, PlayerState ai) {
        if (buildTimer < 6f)
            return;

        buildTimer = 0f;

        if (!hasBuilding(session, "monastery") && ai.hasEnoughResources(150, 75)) {
            build(session, ai, "monastery", 11f, -3f, 400, 150, 75);
        }
    }

    private void rebuildMilitary(GameSession session, PlayerState ai) {
        if (buildTimer < 6f)
            return;

        buildTimer = 0f;

        if (!hasBuilding(session, "barracks") && ai.hasEnoughResources(200, 50)) {
            build(session, ai, "barracks", 8f, 3f, 300, 200, 50);
            return;
        }

        if (!hasBuilding(session, "archery") && ai.hasEnoughResources(150, 100)) {
            build(session, ai, "archery", 10f, 0f, 300, 150, 100);
        }
    }

    private void buildHouse(GameSession session, PlayerState ai) {
        if (!ai.hasEnoughResources(75, 25))
            return;

        int id = session.generateNextBuildingId();

        session.getBuildings().put(
                id,
                new BuildingState(id, aiPlayerId, "house", 13f + id, 3f, 300)
        );

        ai.spendResources(75, 25);
        ai.addMaxSupply(5);

        System.out.println("[AI] Built house");
    }

    private void build(GameSession session, PlayerState ai, String type, float x, float y, int hp, int gold, int lumber) {
        int id = session.generateNextBuildingId();

        session.getBuildings().put(
                id,
                new BuildingState(id, aiPlayerId, type, x, y, hp)
        );

        ai.spendResources(gold, lumber);

        System.out.println("[AI] Rebuilt " + type);
    }

    private void trainWorker(GameSession session, PlayerState ai) {
        BuildingState monastery = findOwnBuilding(session, "monastery");
        if (monastery == null)
            return;

        UnitStats stats = session.getUnitStats("worker");
        if (stats == null)
            return;

        if (!ai.hasEnoughResources(stats.getGoldCost(), stats.getLumberCost()))
            return;

        if (!ai.hasEnoughSupply(stats.getSupplyCost()))
            return;

        ai.spendResources(stats.getGoldCost(), stats.getLumberCost());
        monastery.enqueueUnit("worker");

        System.out.println("[AI] Training worker");
    }

    private void trainCombatUnits(GameSession session, PlayerState ai) {
        BuildingState barracks = findOwnBuilding(session, "barracks");
        BuildingState archery = findOwnBuilding(session, "archery");

        if (archery != null)
            trainUnit(session, ai, archery, "archer");

        if (barracks != null)
            trainUnit(session, ai, barracks, "swordsman");
    }

    private void trainUnit(GameSession session, PlayerState ai, BuildingState building, String unitType) {
        UnitStats stats = session.getUnitStats(unitType);
        if (stats == null)
            return;

        if (!ai.hasEnoughResources(stats.getGoldCost(), stats.getLumberCost()))
            return;

        if (!ai.hasEnoughSupply(stats.getSupplyCost()))
            return;

        ai.spendResources(stats.getGoldCost(), stats.getLumberCost());
        building.enqueueUnit(unitType);

        System.out.println("[AI] Training " + unitType);
    }

    private void sendWorkersToResources(GameSession session) {
        Integer goldId = null;
        Integer lumberId = null;

        for (ResourceNode r : session.getResourceNodes().values()) {
            if ("gold".equals(r.type))
                goldId = r.id;

            if ("lumber".equals(r.type))
                lumberId = r.id;
        }

        boolean goldTurn = true;

        for (UnitState u : session.getUnits().values()) {
            if (u.getOwnerPlayerId() != aiPlayerId)
                continue;

            if (!"worker".equals(u.getUnitType()))
                continue;

            if (u.getGatherTarget() > 0)
                continue;

            if (goldTurn && goldId != null) {
                u.setGatherTarget(goldId);
                goldTurn = false;
                System.out.println("[AI] Worker " + u.getId() + " gather gold");
            } else if (lumberId != null) {
                u.setGatherTarget(lumberId);
                goldTurn = true;
                System.out.println("[AI] Worker " + u.getId() + " gather lumber");
            }
        }
    }

    private void checkVision(GameSession session) {
        knownEnemyBuildingId = null;
        knownEnemyUnitId = null;

        chooseBestVisibleTarget(session);
    }

    private void chooseBestVisibleTarget(GameSession session) {
        UnitState bestUnit = null;

        for (UnitState enemy : session.getUnits().values()) {
            if (enemy.getOwnerPlayerId() == aiPlayerId)
                continue;

            if (!isVisible(session, enemy.getX(), enemy.getY()))
                continue;

            if ("worker".equals(enemy.getUnitType())) {
                bestUnit = enemy;
                break;
            }

            if (bestUnit == null)
                bestUnit = enemy;
        }

        if (bestUnit != null) {
            knownEnemyUnitId = bestUnit.getId();
            return;
        }

        BuildingState bestBuilding = null;
        int bestPriority = -1;

        for (BuildingState b : session.getBuildings().values()) {
            if (b.getOwnerPlayerId() == aiPlayerId)
                continue;

            if (!isVisible(session, b.getX(), b.getY()))
                continue;

            int priority = buildingPriority(b.getBuildingType());

            if (priority > bestPriority) {
                bestPriority = priority;
                bestBuilding = b;
            }
        }

        if (bestBuilding != null)
            knownEnemyBuildingId = bestBuilding.getId();
    }

    private int buildingPriority(String type) {
        return switch (type) {
            case "archery" -> 5;
            case "barracks" -> 4;
            case "monastery" -> 3;
            case "castle" -> 2;
            case "house" -> 1;
            default -> 0;
        };
    }

    private boolean isVisible(GameSession session, float x, float y) {
        for (UnitState u : session.getUnits().values()) {
            if (u.getOwnerPlayerId() != aiPlayerId)
                continue;

            if (distance(u.getX(), u.getY(), x, y) <= visionRange)
                return true;
        }

        for (BuildingState b : session.getBuildings().values()) {
            if (b.getOwnerPlayerId() != aiPlayerId)
                continue;

            if (distance(b.getX(), b.getY(), x, y) <= visionRange)
                return true;
        }

        return false;
    }

    private boolean hasEnemyNearBase(GameSession session) {
        return findEnemyNearBase(session) != null;
    }

    private UnitState findEnemyNearBase(GameSession session) {
        BuildingState base = findOwnCastle(session);
        if (base == null)
            return null;

        for (UnitState enemy : session.getUnits().values()) {
            if (enemy.getOwnerPlayerId() == aiPlayerId)
                continue;

            if (distance(base.getX(), base.getY(), enemy.getX(), enemy.getY()) <= visionRange)
                return enemy;
        }

        return null;
    }

    private boolean hasWeakArmy(GameSession session) {
        for (UnitState u : session.getUnits().values()) {
            if (u.getOwnerPlayerId() != aiPlayerId)
                continue;

            if ("worker".equals(u.getUnitType()))
                continue;

            if (u.getHp() < u.getMaxHp() * 0.35f)
                return true;
        }

        return false;
    }

    private boolean hasBuilding(GameSession session, String type) {
        return findOwnBuilding(session, type) != null;
    }

    private BuildingState findOwnCastle(GameSession session) {
        return findOwnBuilding(session, "castle");
    }

    private BuildingState findOwnBuilding(GameSession session, String type) {
        for (BuildingState b : session.getBuildings().values()) {
            if (b.getOwnerPlayerId() == aiPlayerId && type.equals(b.getBuildingType()))
                return b;
        }

        return null;
    }

    private UnitState findAnyCombatUnit(GameSession session) {
        for (UnitState u : session.getUnits().values()) {
            if (u.getOwnerPlayerId() == aiPlayerId && !"worker".equals(u.getUnitType()))
                return u;
        }

        return null;
    }

    private UnitState findAnyWorker(GameSession session) {
        for (UnitState u : session.getUnits().values()) {
            if (u.getOwnerPlayerId() == aiPlayerId && "worker".equals(u.getUnitType()))
                return u;
        }

        return null;
    }

    private int countWorkers(GameSession session) {
        int count = 0;

        for (UnitState u : session.getUnits().values()) {
            if (u.getOwnerPlayerId() == aiPlayerId && "worker".equals(u.getUnitType()))
                count++;
        }

        return count;
    }

    private int countCombatUnits(GameSession session) {
        int count = 0;

        for (UnitState u : session.getUnits().values()) {
            if (u.getOwnerPlayerId() == aiPlayerId && !"worker".equals(u.getUnitType()))
                count++;
        }

        return count;
    }

    private float distance(float x1, float y1, float x2, float y2) {
        float dx = x2 - x1;
        float dy = y2 - y1;
        return (float) Math.sqrt(dx * dx + dy * dy);
    }
}