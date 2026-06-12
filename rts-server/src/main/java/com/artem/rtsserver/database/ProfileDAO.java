package com.artem.rtsserver.database;

import org.springframework.stereotype.Component;

import javax.sql.DataSource;
import java.sql.*;

@Component
public class ProfileDAO {

    private final DataSource dataSource;

    public ProfileDAO(DataSource dataSource) {
        this.dataSource = dataSource;
    }

    public String buildProfileJson(int playerId) {
        String profileSql = """
            SELECT 
                p.username,
                p.email,
                p.rating,
                ps.total_game_sessions,
                ps.wins,
                ps.losses,
                ps.draws,
                ps.win_rate,
                ps.total_play_time_seconds,
                ps.total_gold_collected,
                ps.total_lumber_collected,
                ps.total_units_created,
                ps.total_buildings_built
            FROM players p
            LEFT JOIN player_stats ps ON ps.player_id = p.id
            WHERE p.id = ?
        """;

        String historySql = """
            SELECT 
                gs.session_type,
                gs.mode,
                gs.difficulty,
                gs.started_at,
                gs.ended_at,
                gs.duration_seconds,
                gsp.result
            FROM game_session_players gsp
            JOIN game_sessions gs ON gs.id = gsp.game_session_id
            WHERE gsp.player_id = ?
            ORDER BY gs.started_at DESC
            LIMIT 10
        """;

        try (Connection conn = dataSource.getConnection()) {
            StringBuilder sb = new StringBuilder();

            try (PreparedStatement ps = conn.prepareStatement(profileSql)) {
                ps.setInt(1, playerId);

                try (ResultSet rs = ps.executeQuery()) {
                    if (!rs.next()) {
                        return "{\"type\":\"profile_error\",\"message\":\"profile_not_found\"}";
                    }

                    sb.append("{\"type\":\"profile\"");
                    sb.append(",\"username\":\"").append(escape(rs.getString("username"))).append("\"");
                    sb.append(",\"email\":\"").append(escape(rs.getString("email"))).append("\"");
                    sb.append(",\"rating\":").append(rs.getInt("rating"));
                    sb.append(",\"totalMatches\":").append(rs.getInt("total_game_sessions"));
                    sb.append(",\"wins\":").append(rs.getInt("wins"));
                    sb.append(",\"losses\":").append(rs.getInt("losses"));
                    sb.append(",\"draws\":").append(rs.getInt("draws"));
                    sb.append(",\"winRate\":").append(rs.getDouble("win_rate"));
                    sb.append(",\"totalPlayTimeSeconds\":").append(rs.getInt("total_play_time_seconds"));
                    sb.append(",\"totalGoldCollected\":").append(rs.getInt("total_gold_collected"));
                    sb.append(",\"totalLumberCollected\":").append(rs.getInt("total_lumber_collected"));
                    sb.append(",\"totalUnitsCreated\":").append(rs.getInt("total_units_created"));
                    sb.append(",\"totalBuildingsBuilt\":").append(rs.getInt("total_buildings_built"));
                }
            }

            sb.append(",\"matches\":[");

            boolean first = true;

            try (PreparedStatement ps = conn.prepareStatement(historySql)) {
                ps.setInt(1, playerId);

                try (ResultSet rs = ps.executeQuery()) {
                    while (rs.next()) {
                        if (!first)
                            sb.append(",");

                        first = false;

                        sb.append("{");
                        sb.append("\"sessionType\":\"").append(escape(rs.getString("session_type"))).append("\"");
                        sb.append(",\"mode\":\"").append(escape(rs.getString("mode"))).append("\"");
                        sb.append(",\"difficulty\":\"").append(escape(rs.getString("difficulty"))).append("\"");
                        sb.append(",\"result\":\"").append(escape(rs.getString("result"))).append("\"");
                        sb.append(",\"startedAt\":\"").append(escape(String.valueOf(rs.getTimestamp("started_at")))).append("\"");
                        sb.append(",\"durationSeconds\":").append(rs.getInt("duration_seconds"));
                        sb.append("}");
                    }
                }
            }

            sb.append("]}");

            return sb.toString();

        } catch (Exception e) {
            e.printStackTrace();
            return "{\"type\":\"profile_error\",\"message\":\"server_error\"}";
        }
    }

    private String escape(String value) {
        if (value == null) return "";

        return value
                .replace("\\", "\\\\")
                .replace("\"", "\\\"");
    }
}