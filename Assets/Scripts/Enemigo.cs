using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class Enemigo : MonoBehaviour
{
    //requisitos del enemigo
    public Transform player;
    public GameObject hospital;
    public GameObject ammoBox;
    public GameObject bulletEnemy;
    public Transform bulletSpawn;
    public TMP_Text enemyLife;
    public GameObject losingPanel;


    //variables
    public NavMeshAgent agent;
    public int maxAmmo = 8;
    public int currentAmmo;
    public float shootCooldown = 1.5f;
    public float shootRange = 8f;
    public float bulletSpeed = 10f;
    public int maxHealth = 100;
    public int currentHealth;
    public bool canShoot = true;
    public bool isReloading = false;
    public bool isHealing = false;
    public float distanceDifferential = 5f;
    //fuzzy
    private float fuzzyPlayerHealth;
    private float fuzzyAmmo;
    private float fuzzyDistancePlayer;
    private float fuzzyDistanceAmmo;
    private float fuzzyDistanceHealth;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentAmmo = maxAmmo;
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (player == null)
            return;

        Fuzzify();
        enemyLife.text = "Enemy Life: " + fuzzyPlayerHealth.ToString();
        Elections();
        if (!isReloading && !isHealing && currentAmmo > 0 && Vector3.Distance(transform.position, player.position) <= shootRange)
        {
            Shoot();
        }
    }

    private void Fuzzify()
    {
        fuzzyPlayerHealth = (currentHealth * 100) / maxHealth;
        fuzzyAmmo = (currentAmmo * 100) / maxAmmo;
        fuzzyDistancePlayer = Vector3.Distance(transform.position, player.position);
        fuzzyDistanceAmmo = Vector3.Distance(transform.position, ammoBox.transform.position);
        fuzzyDistanceHealth = Vector3.Distance(transform.position, hospital.transform.position);
    }

    private void Elections()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool isNearAmmoStation = fuzzyDistanceAmmo <= 15f;
        bool isNearHealthStation = fuzzyDistanceHealth <= 15f;

        // Mantener distancia con el jugador
        if (distanceToPlayer > distanceDifferential)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.SetDestination(transform.position);
        }

        // Buscar recargar solo si la munición es baja y no está cerca de la estación de munición
        if (currentAmmo <= 2 && !isNearAmmoStation && distanceToPlayer > distanceDifferential && fuzzyDistanceAmmo > 20f)
        {
            SerchAmmo();
            return;
        }

        // Buscar curarse solo si la salud es baja y no está cerca de la estación de salud
        if (currentHealth <= 30 && !isNearHealthStation)
        {
            AidKit();
            Scape();
            return;
        }

        // Buscar curarse si está cerca de la estación de salud, solo si no se cumple la condición anterior
        if (currentHealth <= 50 && isNearHealthStation && distanceToPlayer > distanceDifferential && fuzzyDistanceHealth > 20f)
        {
            AidKit();
            Scape();
            return;
        }

        // Alejarse lo máximo posible del jugador para buscar munición solo si no se cumple ninguna de las condiciones anteriores
        if (currentAmmo <= maxAmmo / 2 && !isNearHealthStation && distanceToPlayer > distanceDifferential)
        {
            Scape();
            return;
        }

        // Alejarse lo máximo posible del jugador si la munición y la vida están al máximo, solo si no se cumple ninguna de las condiciones anteriores
        if (currentAmmo == maxAmmo && currentHealth == maxHealth && distanceToPlayer > distanceDifferential)
        {
            Scape();
            return;
        }
    }


    private void Scape()
    {
        Vector3 scapeDirection = transform.position - player.position;
        scapeDirection.y = 0;
        scapeDirection.Normalize();
        Vector3 scapeZone = transform.position - player.position;
        agent.SetDestination(scapeZone);
    }

    private void Shoot()
    {
        if (canShoot && currentAmmo > 0)
        {
            GameObject bullet = Instantiate(bulletEnemy, bulletSpawn.position, Quaternion.identity);

            Vector3 direction = (player.position - bulletSpawn.position).normalized;

            Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
            if (bulletRigidbody != null)
            {
                bulletRigidbody.velocity = direction * bulletSpeed;
            }

            Destroy(bullet, 3f);

            currentAmmo--;

            StartCoroutine(ShootCooldown());
        }
    }

    private void AidKit()
    {
        agent.SetDestination(hospital.transform.position);
        if (Vector3.Distance(transform.position, hospital.transform.position) < 1f)
        {
            StartCoroutine(HealOverTime());
        }
    }

    private void SerchAmmo()
    {
        agent.SetDestination(ammoBox.transform.position);
    }

    private void RestockAmmo()
    {
        currentAmmo = maxAmmo;
    }

    public void Heal()
    {
        currentHealth = maxHealth;
    }

    private IEnumerator ShootCooldown()
    {
        canShoot = false;
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }

    private IEnumerator Reload()
    {
        isReloading = true;

        yield return new WaitForSeconds(shootCooldown);

        RestockAmmo();

        isReloading = false;

        agent.isStopped = false;
    }

    

    private void Die()
    {
        Time.timeScale = 0f;
        losingPanel.SetActive(true);
        Debug.Log("Game Over");
    }

    public void Restart()
    {
        StartCoroutine(RestartCoroutine());
    }

    private IEnumerator RestartCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == ammoBox && !isReloading)
        {
            agent.isStopped = true;
            StartCoroutine(Reload());
        }
        else if (other.CompareTag("PlayerBullet"))
        {
            TakeDamage(20);
            Destroy(other.gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HealOverTime()
    {
        isHealing = true;
        while (currentHealth < maxHealth)
        {
            yield return new WaitForSeconds(1f);
            currentHealth += 20;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }
        isHealing = false;
    }

    public void InitialMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
