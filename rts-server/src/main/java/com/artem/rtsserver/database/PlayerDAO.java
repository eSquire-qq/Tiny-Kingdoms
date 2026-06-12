package com.artem.rtsserver.database;

import com.artem.rtsserver.match.PlayerState;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import javax.sql.DataSource;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;

@Component
public class PlayerDAO {

    private final DataSource dataSource;

    @Autowired
    public PlayerDAO(DataSource dataSource) {
        this.dataSource = dataSource;
    }

    public PlayerState loadPlayer(int playerId) {
        String sql = """
            SELECT 
                gold_amount,
                lumber_amount,
                used_supply,
                max_supply
            FROM player_resources
            WHERE player_id = ?
        """;

        try (
            Connection conn = dataSource.getConnection();
            PreparedStatement stmt = conn.prepareStatement(sql)
        ) {
            stmt.setInt(1, playerId);

            try (ResultSet rs = stmt.executeQuery()) {
                if (rs.next()) {
                    return new PlayerState(
                        playerId,
                        rs.getInt("gold_amount"),
                        rs.getInt("lumber_amount"),
                        rs.getInt("used_supply"),
                        rs.getInt("max_supply")
                    );
                }
            }

        } catch (Exception e) {
            System.err.println("[PlayerDAO] loadPlayer failed for playerId=" + playerId);
            e.printStackTrace();
        }

        return null;
    }

    public void saveResources(int playerId, int gold, int lumber, int usedSupply, int maxSupply) {
        String sql = """
            INSERT INTO player_resources (
                player_id,
                gold_amount,
                lumber_amount,
                max_supply,
                used_supply
            )
            VALUES (?, ?, ?, ?, ?)
            ON DUPLICATE KEY UPDATE
                gold_amount = VALUES(gold_amount),
                lumber_amount = VALUES(lumber_amount),
                max_supply = VALUES(max_supply),
                used_supply = VALUES(used_supply)
        """;

        try (
            Connection conn = dataSource.getConnection();
            PreparedStatement stmt = conn.prepareStatement(sql)
        ) {
            stmt.setInt(1, playerId);
            stmt.setInt(2, gold);
            stmt.setInt(3, lumber);
            stmt.setInt(4, maxSupply);
            stmt.setInt(5, usedSupply);

            stmt.executeUpdate();

        } catch (Exception e) {
            System.err.println("[PlayerDAO] saveResources failed for playerId=" + playerId);
            e.printStackTrace();
        }
    }

    public void createDefaultResourcesIfMissing(int playerId) {
        String sql = """
            INSERT IGNORE INTO player_resources (
                player_id,
                gold_amount,
                lumber_amount,
                max_supply,
                used_supply
            )
            VALUES (?, 500, 200, 10, 0)
        """;

        try (
            Connection conn = dataSource.getConnection();
            PreparedStatement stmt = conn.prepareStatement(sql)
        ) {
            stmt.setInt(1, playerId);
            stmt.executeUpdate();

        } catch (Exception e) {
            System.err.println("[PlayerDAO] createDefaultResourcesIfMissing failed for playerId=" + playerId);
            e.printStackTrace();
        }
    }
}