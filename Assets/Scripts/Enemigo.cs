using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class Enemigo : MonoBehaviour
{
    // Requisitos del enemigo
    public Transform player; 
    public GameObject hospital; 
    public GameObject ammoBox; 
    public GameObject bulletEnemy; 
    public Transform bulletSpawn; 
    public TMP_Text enemyLife; 
    public GameObject losingPanel; 

    // Variables del enemigo
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
    public float closeRange = 4f; 
    public float safeDistance = 15f; 
    public float movementInterval = 4f; 

    // Variables fuzzy para la toma de decisiones
    private float fuzzyPlayerHealth; 
    private float fuzzyAmmo; 
    private float fuzzyDistancePlayer; 
    private float fuzzyDistanceAmmo; 
    private float fuzzyDistanceHealth; 

    // Inicialización
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>(); 
        currentAmmo = maxAmmo; 
        currentHealth = maxHealth; 
        StartCoroutine(UpdateMovement()); 
    }

    // Actualización
    private void Update()
    {
        // verificar si existe el player
        if (player == null)
            return;

        Fuzzify(); // Calcular variables fuzzy
        enemyLife.text = "Enemy Life: " + fuzzyPlayerHealth.ToString(); // Actualizar el texto de la vida del enemigo
        Elections(); // Realizar acciones en función de las variables fuzzy
    }

    // Calcular variables fuzzy
    private void Fuzzify()
    {
        fuzzyPlayerHealth = (currentHealth * 100) / maxHealth; // Calcular salud del jugador
        fuzzyAmmo = (currentAmmo * 100) / maxAmmo; // Calcular cantidad de munición
        fuzzyDistancePlayer = Vector3.Distance(transform.position, player.position); // Calcular distancia al jugador
        fuzzyDistanceAmmo = Vector3.Distance(transform.position, ammoBox.transform.position); // Calcular distancia a la caja de munición
        fuzzyDistanceHealth = Vector3.Distance(transform.position, hospital.transform.position); // Calcular distancia al hospital
    }

    // Tomar decisiones en función de las variables fuzzy
    private void Elections()
    {
        // Escapar cuando la vida y la munición estén al máximo
        if (fuzzyPlayerHealth == 100 && fuzzyAmmo == 100)
        {
            Scape();
            return;
        }

        // Disparar si el jugador está en rango muy cercano
        if (fuzzyDistancePlayer <= closeRange && currentAmmo > 0)
        {
            Shoot();
            return;
        }

        // Buscar curarse si la vida es muy baja
        if (fuzzyPlayerHealth <= 50)
        {
            AidKit();
            return;
        }

        // Buscar recargar si la munición es baja
        if (fuzzyAmmo <= 20)
        {
            SerchAmmo();
            return;
        }

        // Priorizar curarse si la vida es algo baja y el hospital está cerca
        if (fuzzyPlayerHealth <= 70 && fuzzyDistanceHealth <= safeDistance)
        {
            AidKit();
            return;
        }

        // Priorizar la munición si la munición es algo baja y la caja de munición está cerca
        if (fuzzyAmmo <= 50 && fuzzyDistanceAmmo <= safeDistance)
        {
            SerchAmmo();
            return;
        }

        // Mantener distancia con el jugador
        MaintainDistance();
    }

    // Escapar del jugador
    private void Scape()
    {
        Vector3 scapeDirection = transform.position - player.position;
        scapeDirection.y = 0;
        scapeDirection.Normalize();
        Vector3 scapeZone = transform.position + scapeDirection * 10f; // Ajustar la distancia de escape si es necesario
        agent.SetDestination(scapeZone);
    }

    // Disparar al jugador
    private void Shoot()
    {
        if (canShoot && currentAmmo > 0)
        {
            GameObject bullet = Instantiate(bulletEnemy, bulletSpawn.position, Quaternion.identity); // Instanciar la bala del enemigo
            Vector3 direction = (player.position - bulletSpawn.position).normalized; // Calcular dirección del disparo
            Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>(); // Obtener componente Rigidbody de la bala
            if (bulletRigidbody != null)
            {
                bulletRigidbody.velocity = direction * bulletSpeed; // Establecer velocidad de la bala
            }
            Destroy(bullet, 3f); // Destruir la bala después de un tiempo
            currentAmmo--; // Reducir la munición
            StartCoroutine(ShootCooldown()); // Iniciar el cooldown de disparo
        }
    }

    // Buscar el hospital
    private void AidKit()
    {
        agent.SetDestination(hospital.transform.position); // Establecer destino al hospital
        if (Vector3.Distance(transform.position, hospital.transform.position) < 1f) // Si está cerca del hospital
        {
            StartCoroutine(HealOverTime()); // se cura con el tiempo
        }
    }

    // Buscar munición en la caja de munición
    private void SerchAmmo()
    {
        agent.SetDestination(ammoBox.transform.position); // Establecer destino a la caja de munición


    }

    // Mantener una distancia segura con el jugador
    private void MaintainDistance()
    {
        if (fuzzyDistancePlayer < safeDistance)
        {
            Scape(); // Si está muy cerca, escapar
        }
        else if (fuzzyDistancePlayer > safeDistance * 1.5)
        {
            agent.SetDestination(player.position); // Si está muy lejos, acercarse al jugador
        }
    }

    // Recargar munición
    private void RestockAmmo()
    {
        currentAmmo = maxAmmo; // Restablecer la munición al máximo
    }

    // Curar  al enemigo
    public void Heal()
    {
        currentHealth = maxHealth; // Establecer la vida al máximo
    }

    // Rutina para el cooldown de disparo
    private IEnumerator ShootCooldown()
    {
        canShoot = false; 
        yield return new WaitForSeconds(shootCooldown); // Esperar el tiempo de cooldown
        canShoot = true; 
    }

    // Rutina para la recarga de munición
    private IEnumerator Reload()
    {
        isReloading = true; //está recargando
        yield return new WaitForSeconds(shootCooldown); // tiempo de recarga
        RestockAmmo(); // Recargar munición
        isReloading = false; 
        agent.isStopped = false; // Reactivar movimiento
    }

    //muerte del enemigo
    private void Die()
    {
        Time.timeScale = 0f; //Pausar el tiempo
        losingPanel.SetActive(true); //Activar panel de derrota
        Debug.Log("Game Over"); //Imprimir en consola
    }

    // Reiniciar el juego
    public void Restart()
    {
        StartCoroutine(RestartCoroutine()); // Iniciar la rutina de reinicio
    }

    // Rutina para reiniciar el juego
    private IEnumerator RestartCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.1f); // Esperar un pequeño tiempo
        Time.timeScale = 1f; // Reanudar el tiempo
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Cargar la escena actual
    }

    // Manejar colisiones del enemigo
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == ammoBox && !isReloading) // Si colisiona con la caja de munición y no está recargando
        {
            agent.isStopped = true; // Detener movimiento
            StartCoroutine(Reload()); // Iniciar recarga
        }
        else if (other.CompareTag("PlayerBullet")) // Si colisiona con una bala del jugador
        {
            TakeDamage(20); // Recibir daño
            Destroy(other.gameObject); // Destruir la bala
        }
    }

    // Recibir daño
    public void TakeDamage(int damage)
    {
        currentHealth -= damage; // Reducir la vida
        if (currentHealth <= 0) // Si la vida es igual o menor a cero
        {
            Die(); // Morir
        }
    }

    // Curarse gradualmente con el tiempo
    private IEnumerator HealOverTime()
    {
        isHealing = true; // Indicar que está siendo curado
        while (currentHealth < maxHealth) // Mientras la vida no esté al máximo
        {
            yield return new WaitForSeconds(1f); // Esperar un segundo
            currentHealth += 20; // Incrementar la vida
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Limitar la vida al máximo
        }
        isHealing = false; 
    }

    // Actualizar el movimiento del enemigo
    private IEnumerator UpdateMovement()
    {
        while (true) // Bucle infinito
        {
            if (!isHealing && !isReloading) // Si no está siendo curado ni recargando
            {
                if (fuzzyDistancePlayer >= safeDistance) // Si la distancia al jugador es mayor o igual a la distancia segura
                {
                    Vector3 randomDirection = Random.insideUnitSphere * safeDistance; // Obtener una dirección aleatoria dentro de un radio seguro
                    randomDirection += transform.position; // Ajustar la dirección respecto a la posición actual
                    NavMeshHit navHit; // Información sobre el punto de navegación alcanzado
                    NavMesh.SamplePosition(randomDirection, out navHit, safeDistance, -1); // Obtener un punto de navegación válido
                    agent.SetDestination(navHit.position); // Establecer destino hacia el punto de navegación
                }
            }
            yield return new WaitForSeconds(movementInterval); // Esperar un intervalo antes de volver a calcular el movimiento
        }
    }

    // Cargar menú inicial
    public void InitialMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1); // Cargar la escena del menú inicial
    }

    // Salir del juego
    public void Quit()
    {
        Application.Quit(); // Salir de la aplicación
    }
}
