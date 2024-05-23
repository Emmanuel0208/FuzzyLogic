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

    // Inicializaci�n
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>(); 
        currentAmmo = maxAmmo; 
        currentHealth = maxHealth; 
        StartCoroutine(UpdateMovement()); 
    }

    // Actualizaci�n
    private void Update()
    {
        // verificar si existe el player
        if (player == null)
            return;

        Fuzzify(); // Calcular variables fuzzy
        enemyLife.text = "Enemy Life: " + fuzzyPlayerHealth.ToString(); // Actualizar el texto de la vida del enemigo
        Elections(); // Realizar acciones en funci�n de las variables fuzzy
    }

    // Calcular variables fuzzy
    private void Fuzzify()
    {
        fuzzyPlayerHealth = (currentHealth * 100) / maxHealth; // Calcular salud del jugador
        fuzzyAmmo = (currentAmmo * 100) / maxAmmo; // Calcular cantidad de munici�n
        fuzzyDistancePlayer = Vector3.Distance(transform.position, player.position); // Calcular distancia al jugador
        fuzzyDistanceAmmo = Vector3.Distance(transform.position, ammoBox.transform.position); // Calcular distancia a la caja de munici�n
        fuzzyDistanceHealth = Vector3.Distance(transform.position, hospital.transform.position); // Calcular distancia al hospital
    }

    // Tomar decisiones en funci�n de las variables fuzzy
    private void Elections()
    {
        // Escapar cuando la vida y la munici�n est�n al m�ximo
        if (fuzzyPlayerHealth == 100 && fuzzyAmmo == 100)
        {
            Scape();
            return;
        }

        // Disparar si el jugador est� en rango muy cercano
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

        // Buscar recargar si la munici�n es baja
        if (fuzzyAmmo <= 20)
        {
            SerchAmmo();
            return;
        }

        // Priorizar curarse si la vida es algo baja y el hospital est� cerca
        if (fuzzyPlayerHealth <= 70 && fuzzyDistanceHealth <= safeDistance)
        {
            AidKit();
            return;
        }

        // Priorizar la munici�n si la munici�n es algo baja y la caja de munici�n est� cerca
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
            Vector3 direction = (player.position - bulletSpawn.position).normalized; // Calcular direcci�n del disparo
            Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>(); // Obtener componente Rigidbody de la bala
            if (bulletRigidbody != null)
            {
                bulletRigidbody.velocity = direction * bulletSpeed; // Establecer velocidad de la bala
            }
            Destroy(bullet, 3f); // Destruir la bala despu�s de un tiempo
            currentAmmo--; // Reducir la munici�n
            StartCoroutine(ShootCooldown()); // Iniciar el cooldown de disparo
        }
    }

    // Buscar el hospital
    private void AidKit()
    {
        agent.SetDestination(hospital.transform.position); // Establecer destino al hospital
        if (Vector3.Distance(transform.position, hospital.transform.position) < 1f) // Si est� cerca del hospital
        {
            StartCoroutine(HealOverTime()); // se cura con el tiempo
        }
    }

    // Buscar munici�n en la caja de munici�n
    private void SerchAmmo()
    {
        agent.SetDestination(ammoBox.transform.position); // Establecer destino a la caja de munici�n


    }

    // Mantener una distancia segura con el jugador
    private void MaintainDistance()
    {
        if (fuzzyDistancePlayer < safeDistance)
        {
            Scape(); // Si est� muy cerca, escapar
        }
        else if (fuzzyDistancePlayer > safeDistance * 1.5)
        {
            agent.SetDestination(player.position); // Si est� muy lejos, acercarse al jugador
        }
    }

    // Recargar munici�n
    private void RestockAmmo()
    {
        currentAmmo = maxAmmo; // Restablecer la munici�n al m�ximo
    }

    // Curar  al enemigo
    public void Heal()
    {
        currentHealth = maxHealth; // Establecer la vida al m�ximo
    }

    // Rutina para el cooldown de disparo
    private IEnumerator ShootCooldown()
    {
        canShoot = false; 
        yield return new WaitForSeconds(shootCooldown); // Esperar el tiempo de cooldown
        canShoot = true; 
    }

    // Rutina para la recarga de munici�n
    private IEnumerator Reload()
    {
        isReloading = true; //est� recargando
        yield return new WaitForSeconds(shootCooldown); // tiempo de recarga
        RestockAmmo(); // Recargar munici�n
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
        yield return new WaitForSecondsRealtime(0.1f); // Esperar un peque�o tiempo
        Time.timeScale = 1f; // Reanudar el tiempo
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Cargar la escena actual
    }

    // Manejar colisiones del enemigo
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == ammoBox && !isReloading) // Si colisiona con la caja de munici�n y no est� recargando
        {
            agent.isStopped = true; // Detener movimiento
            StartCoroutine(Reload()); // Iniciar recarga
        }
        else if (other.CompareTag("PlayerBullet")) // Si colisiona con una bala del jugador
        {
            TakeDamage(20); // Recibir da�o
            Destroy(other.gameObject); // Destruir la bala
        }
    }

    // Recibir da�o
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
        isHealing = true; // Indicar que est� siendo curado
        while (currentHealth < maxHealth) // Mientras la vida no est� al m�ximo
        {
            yield return new WaitForSeconds(1f); // Esperar un segundo
            currentHealth += 20; // Incrementar la vida
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Limitar la vida al m�ximo
        }
        isHealing = false; 
    }

    // Actualizar el movimiento del enemigo
    private IEnumerator UpdateMovement()
    {
        while (true) // Bucle infinito
        {
            if (!isHealing && !isReloading) // Si no est� siendo curado ni recargando
            {
                if (fuzzyDistancePlayer >= safeDistance) // Si la distancia al jugador es mayor o igual a la distancia segura
                {
                    Vector3 randomDirection = Random.insideUnitSphere * safeDistance; // Obtener una direcci�n aleatoria dentro de un radio seguro
                    randomDirection += transform.position; // Ajustar la direcci�n respecto a la posici�n actual
                    NavMeshHit navHit; // Informaci�n sobre el punto de navegaci�n alcanzado
                    NavMesh.SamplePosition(randomDirection, out navHit, safeDistance, -1); // Obtener un punto de navegaci�n v�lido
                    agent.SetDestination(navHit.position); // Establecer destino hacia el punto de navegaci�n
                }
            }
            yield return new WaitForSeconds(movementInterval); // Esperar un intervalo antes de volver a calcular el movimiento
        }
    }

    // Cargar men� inicial
    public void InitialMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1); // Cargar la escena del men� inicial
    }

    // Salir del juego
    public void Quit()
    {
        Application.Quit(); // Salir de la aplicaci�n
    }
}
