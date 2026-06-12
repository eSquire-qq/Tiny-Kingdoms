package com.artem.rtsserver.database;

import org.springframework.stereotype.Component;

import javax.sql.DataSource;
import java.sql.*;

@Component
public class MatchHistoryDAO {

    private final DataSource dataSource;

    public MatchHistoryDAO(DataSource dataSource) {
        this.dataSource = dataSource;
    }

    public int createGameSession(String sessionType, String mode, String difficulty) {
        String sql = """
            INSERT INTO game_sessions (
                session_type,
                mode,
                status,
                difficulty,
                started_at,
                map_name,
                game_version
            )
            VALUES (?, ?, 'in_progress', ?, CURRENT_TIMESTAMP, 'Default Map', '1.0')
        """;

        try (
            Connection conn = dataSource.getConnection();
            PreparedStatement ps = conn.prepareStatement(sql, Statement.RETURN_GENERATED_KEYS)
        ) {
            ps.setString(1, sessionType);
            ps.setString(2, mode);

            if (difficulty == null)
                ps.setNull(3, Types.VARCHAR);
            else
                ps.setString(3, difficulty);

            ps.executeUpdate();

            try (ResultSet keys = ps.getGeneratedKeys()) {
                if (keys.next())
                    return keys.getInt(1);
            }

        } catch (Exception e) {
            e.printStackTrace();
        }

        return -1;
    }

    public void finishGameSession(int gameSessionId, Integer winnerPlayerId, String winnerSide, int durationSeconds) {
        if (gameSessionId <= 0) return;

        String sql = """
            UPDATE game_sessions
            SET 
                status = 'finished',
                ended_at = CURRENT_TIMESTAMP,
                duration_seconds = ?,
                winner_player_id = ?,
                winner_side = ?
            WHERE id = ?
        """;

        try (
            Connection conn = dataSource.getConnection();
            PreparedStatement ps = conn.prepareStatement(sql)
        ) {
            ps.setInt(1, durationSeconds);

            if (winnerPlayerId == null)
                ps.setNull(2, Types.INTEGER);
            else
                ps.setInt(2, winnerPlayerId);

            ps.setString(3, winnerSide);
            ps.setInt(4, gameSessionId);

            ps.executeUpdate();

        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void saveSessionPlayer(
            int gameSessionId,
            Integer playerId,
            boolean isAi,
            String result,
            int goldCollected,
            int lumberCollected,
            int unitsCreated,
            int unitsKilled,
            int buildingsBuilt,
            int buildingsDestroyed
    ) {
        if (gameSessionId <= 0) return;

        String sql = """
            INSERT INTO game_session_players (
                game_session_id,
                player_id,
                team_id,
                is_ai,
                result,
                gold_collected,
                lumber_collected,
                units_created,
                units_killed,
                buildings_built,
                buildings_destroyed
            )
            VALUES (?, ?, 1, ?, ?, ?, ?, ?, ?, ?, ?)
        """;

        try (
            Connection conn = dataSource.getConnection();
            PreparedStatement ps = conn.prepareStatement(sql)
        ) {
            ps.setInt(1, gameSessionId);

            if (playerId == null)
                ps.setNull(2, Types.INTEGER);
            else
                ps.setInt(2, playerId);

            ps.setBoolean(3, isAi);
            ps.setString(4, result);
            ps.setInt(5, goldCollected);
            ps.setInt(6, lumberCollected);
            ps.setInt(7, unitsCreated);
            ps.setInt(8, unitsKilled);
            ps.setInt(9, buildingsBuilt);
            ps.setInt(10, buildingsDestroyed);

            ps.executeUpdate();

        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void updatePlayerStats(
            int playerId,
            String result,
            int durationSeconds,
            int goldCollected,
            int lumberCollected,
            int unitsCreated,
            int buildingsBuilt
    ) {
        if (playerId <= 0) return;

        String insertSql = """
            INSERT IGNORE INTO player_stats (player_id)
            VALUES (?)
        """;

        String updateSql = """
            UPDATE player_stats
            SET
                total_game_sessions = total_game_sessions + 1,
                wins = wins + ?,
                losses = losses + ?,
                draws = draws + ?,
                total_play_time_seconds = total_play_time_seconds + ?,
                total_gold_collected = total_gold_collected + ?,
                total_lumber_collected = total_lumber_collected + ?,
                total_units_created = total_units_created + ?,
                total_buildings_built = total_buildings_built + ?,
                win_rate = ROUND((wins / NULLIF(total_game_sessions, 0)) * 100, 2),
                updated_at = CURRENT_TIMESTAMP
            WHERE player_id = ?
        """;

        int winAdd = "win".equals(result) ? 1 : 0;
        int lossAdd = "loss".equals(result) ? 1 : 0;
        int drawAdd = "draw".equals(result) ? 1 : 0;

        try (Connection conn = dataSource.getConnection()) {
            try (PreparedStatement ps = conn.prepareStatement(insertSql)) {
                ps.setInt(1, playerId);
                ps.executeUpdate();
            }

            try (PreparedStatement ps = conn.prepareStatement(updateSql)) {
                ps.setInt(1, winAdd);
                ps.setInt(2, lossAdd);
                ps.setInt(3, drawAdd);
                ps.setInt(4, durationSeconds);
                ps.setInt(5, goldCollected);
                ps.setInt(6, lumberCollected);
                ps.setInt(7, unitsCreated);
                ps.setInt(8, buildingsBuilt);
                ps.setInt(9, playerId);

                ps.executeUpdate();
            }

        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}